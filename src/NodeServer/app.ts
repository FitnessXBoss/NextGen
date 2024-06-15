import express from 'express';
import { getHttpEndpoint } from "@orbs-network/ton-access";
import { mnemonicToWalletKey } from "@ton/crypto";
import { TonClient, WalletContractV4 } from "@ton/ton";
import { v4 as uuidv4 } from 'uuid';
import * as QRCode from 'qrcode';
import * as fs from 'fs';
import { Client } from 'pg';
import axios from 'axios';
import { toNano, fromNano } from '@ton/core';

const API_KEY = '6b347998e2359bc8039728754ac176830c60cde01bcad1170e1f058239bd4a33';

const pgClient = new Client({
    host: '192.168.0.183',
    port: 5432,
    database: 'SecurityData',
    user: 'postgres',
    password: '72745621008',
});

pgClient.connect();

const app = express();
const port = 3000;

app.get('/generate-qrcode', async (req, res) => {
    const amount = req.query.amount;
    const uniqueId = uuidv4();
    const mnemonic = "coral about client mandate inside shine inhale tumble royal garden crouch cook answer flight grape poverty inhale west spoil million stable exit shell elephant";
    const key = await mnemonicToWalletKey(mnemonic.split(" "));
    const wallet = WalletContractV4.create({ publicKey: key.publicKey, workchain: 0 });
    const expectedReturnAmount = toNano(parseFloat(amount as string)); // Преобразование суммы к числу
    const tonLink = generateTonLink(wallet.address.toString(), expectedReturnAmount, uniqueId);
    await generateQRCode(tonLink);

    res.send(`http://localhost:3000/qrcode.png`);
});

app.listen(port, () => {
    console.log(`Server running at http://localhost:${port}/`);
});

async function main() {
    // Generate a unique value
    const uniqueId = uuidv4();
    console.log(`Unique ID: ${uniqueId}`);

    // Open wallet v4 (ensure correct wallet version)
    const mnemonic = "coral about client mandate inside shine inhale tumble royal garden crouch cook answer flight grape poverty inhale west spoil million stable exit shell elephant";
    const key = await mnemonicToWalletKey(mnemonic.split(" "));
    const wallet = WalletContractV4.create({ publicKey: key.publicKey, workchain: 0 });

    // Initialize ton rpc client on the test network
    const endpoint = await getHttpEndpoint({ network: "testnet" });
    const client = new TonClient({ endpoint });
    const contract = client.open(wallet);

    console.log(`Wallet address: ${wallet.address.toString()}`);

    // Ensure the wallet is deployed
    if (!await client.isContractDeployed(wallet.address)) {
        return console.log("Wallet is not deployed");
    }

    // Check wallet balance
    const balanceBefore = await contract.getBalance();
    console.log(`Wallet balance before transfer: ${fromNano(balanceBefore)} TON`);

    const expectedReturnAmount = toNano(1);  // 1 TON expected return amount from user

    // Generate QR code
    const tonLink = generateTonLink(wallet.address.toString(), expectedReturnAmount, uniqueId);
    await generateQRCode(tonLink);

    // Wait for user transfer
    console.log(`Awaiting transfer from user to address: ${wallet.address.toString()} with amount: ${fromNano(expectedReturnAmount)} TON with comment: ${uniqueId}`);

    let received = false;

    while (!received) {
        try {
            const transactions = await getTransactions(wallet.address.toString());

            for (const transaction of transactions) {
                const transactionAmount = BigInt(transaction.value);
                const comment = transaction.comment;

                if (transactionAmount >= expectedReturnAmount && comment === uniqueId) {
                    received = true;
                    console.log(`Received amount: ${fromNano(transactionAmount)} TON, comment: ${comment}`);
                    console.log("Comment matches unique ID and amount is correct: TRUE");
                    console.log("Car sold");

                    const viewLink = `https://testnet.toncenter.com/api/v2/getTransactions?address=${wallet.address.toString()}&limit=1`;

                    // Record data in the database
                    await pgClient.query(
                        'INSERT INTO payments (address, amount, comment, sender_wallet, transaction_id, timestamp, view_link) VALUES ($1, $2, $3, $4, $5, $6, $7)',
                        [wallet.address.toString(), fromNano(transactionAmount), comment, transaction.sender, transaction.transaction_id.hash, new Date(transaction.utime * 1000), viewLink]
                    );

                    console.log("Transaction successfully recorded in the database.");

                    // Notify C# application
                    await notifyCSharpApp(transaction);

                    process.exit(0); // Exit the program
                }
            }

            if (!received) {
                await dynamicSleep(2000); // Check every 2 seconds with a countdown
            }
        } catch (error) {
            console.error('Error fetching transaction details:', error);
            await dynamicSleep(2000); // Check every 2 seconds with a countdown
        }
    }
}

main();

function generateTonLink(address: string, amount: bigint, comment: string): string {
    const amountInNano = amount.toString();
    return `ton://transfer/${address}?amount=${amountInNano}&text=${comment}`;
}

async function generateQRCode(text: string) {
    try {
        await QRCode.toFile('qrcode.png', text);
        console.log('QR code saved as qrcode.png');
    } catch (err) {
        console.error('Error generating QR code', err);
    }
}

function sleep(ms: number) {
    return new Promise(resolve => setTimeout(resolve, ms));
}

async function dynamicSleep(ms: number) {
    for (let remaining = ms; remaining > 0; remaining -= 1000) {
        process.stdout.write(`\rNext check in ${remaining / 1000} seconds...`);
        await sleep(1000);
    }
    process.stdout.write('\r                              \r'); // Clear the line after the timer finishes
}

async function getTransactions(walletAddress: string) {
    const apiEndpoint = 'https://testnet.toncenter.com/api/v2/getTransactions';
    const url = `${apiEndpoint}?address=${walletAddress}&limit=1`; // Check one transaction

    for (let attempt = 0; attempt < 3; attempt++) {
        try {
            const response = await axios.get(url, {
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${API_KEY}` // Add API key in the header
                }
            });

            if (response.status !== 200) {
                throw new Error(`Error: ${response.statusText}`);
            }

            const data: any = response.data;
            const transactions = data.result; // All transactions from the result array

            if (transactions && transactions.length > 0) {
                return transactions.map((transaction: any) => {
                    const value = transaction.in_msg?.value || '0';
                    const comment = transaction.in_msg?.message;
                    const sender = transaction.in_msg?.source;
                    const transactionId = transaction.transaction_id;
                    const utime = transaction.utime;
                    return { value, comment, sender, transaction_id: transactionId, utime };
                });
            } else {
                return [];
            }
        } catch (error: unknown) {
            if (error instanceof Error) {
                console.error('Error fetching transaction details:', error.message);
            } else {
                console.error('Unexpected error:', error);
            }
            if (attempt < 2) {
                await sleep(5000); // Delay before retrying
            } else {
                throw error;
            }
        }
    }
}

async function notifyCSharpApp(transaction: any) {
    try {
        await axios.post('http://localhost:5220/api/example/paymentSuccessful', {
            Comment: transaction.comment,
            Amount: transaction.value,
            Sender: transaction.sender,
        });
    } catch (error) {
        console.error('Error notifying C# app:', error);
    }
}
