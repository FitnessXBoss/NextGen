import express from 'express';
import { mnemonicToWalletKey } from "@ton/crypto";
import { WalletContractV4 } from "@ton/ton";
import { toNano, fromNano } from '@ton/core';
import { v4 as uuidv4 } from 'uuid';
import axios from 'axios';

const app = express();
const port = 3001;

// Глобальные переменные для хранения адреса и комментария
let generatedAddress: string = '';
let generatedComment: string = '';
let expectedReturnAmount: bigint = BigInt(0);

const API_KEY = '6b347998e2359bc8039728754ac176830c60cde01bcad1170e1f058239bd4a33';

app.use(express.json());

app.post('/generate-payment', async (req, res) => {
    const { amount } = req.body;
    const uniqueId = uuidv4();
    const mnemonic = "coral about client mandate inside shine inhale tumble royal garden crouch cook answer flight grape poverty inhale west spoil million stable exit shell elephant";
    const key = await mnemonicToWalletKey(mnemonic.split(" "));
    const wallet = WalletContractV4.create({ publicKey: key.publicKey, workchain: 0 });
    expectedReturnAmount = toNano(parseFloat(amount)); // Преобразование суммы к числу
    const tonLink = generateTonLink(wallet.address.toString(), expectedReturnAmount, uniqueId);

    // Сохраняем адрес и комментарий для последующей проверки
    generatedAddress = wallet.address.toString();
    generatedComment = uniqueId;

    // Выводим комментарий и сумму
    console.log(`Expected comment: ${generatedComment}`);
    console.log(`Expected amount: ${fromNano(expectedReturnAmount)} TON`);

    res.json({
        address: wallet.address.toString(),
        uniqueId,
        amount: expectedReturnAmount.toString(),
        tonLink
    });

    // Запускаем процесс проверки транзакций
    checkForTransactions();
});

async function checkForTransactions() {
    let received = false;

    while (!received) {
        try {
            const transactions = await getTransactions(generatedAddress);

            for (const transaction of transactions) {
                const transactionAmount = BigInt(transaction.value);
                const comment = transaction.comment;

                if (transactionAmount >= expectedReturnAmount && comment === generatedComment) {
                    received = true;
                    console.log(`Received amount: ${fromNano(transactionAmount)} TON, comment: ${comment}`);
                    console.log("Comment matches unique ID and amount is correct: TRUE");
                    console.log("Payment verified successfully");

                    // Дополнительные действия, например, уведомление C# приложения
                    await notifyCSharpApp(transaction);

                    break;
                }
            }

            if (!received) {
                await dynamicSleep(2000); // Проверяем каждые 2 секунды
            }
        } catch (error) {
            if (error instanceof Error) {
                console.error('Error fetching transaction details:', error.message);
            } else {
                console.error('Unexpected error:', error);
            }
            await dynamicSleep(2000); // Проверяем каждые 2 секунды
        }
    }
}

app.listen(port, () => {
    console.log(`Server running at http://localhost:${port}/`);
});

function generateTonLink(address: string, amount: bigint, comment: string): string {
    const amountInNano = amount.toString();
    return `ton://transfer/${address}?amount=${amountInNano}&text=${comment}`;
}

async function getTransactions(walletAddress: string) {
    const apiEndpoint = 'https://testnet.toncenter.com/api/v2/getTransactions';
    const url = `${apiEndpoint}?address=${walletAddress}&limit=5`;  // Проверка нескольких последних транзакций

    try {
        const response = await axios.get(url, {
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${API_KEY}`
            }
        });

        const data = response.data;
        const transactions = data.result;

        return transactions.map((transaction: any) => {
            const value = transaction.in_msg?.value || '0';
            const comment = transaction.in_msg?.message;
            const sender = transaction.in_msg?.source;
            const transactionId = transaction.transaction_id;
            const utime = transaction.utime;
            return { value, comment, sender, transaction_id: transactionId, utime };
        });
    } catch (error) {
        if (error instanceof Error) {
            console.error('Error fetching transaction details:', error.message);
        } else {
            console.error('Unexpected error:', error);
        }
        throw error;
    }
}

async function notifyCSharpApp(transaction: any) {
    console.log('Notifying C# app with transaction data:', transaction);
    try {
        const response = await axios.post('http://localhost:5220/api/payment/paymentSuccessful', {
            Comment: transaction.comment,
            Amount: transaction.value,
            Sender: transaction.sender,
            CarId: 1 // Замените на реальное значение CarId
        }, {
            timeout: 10000 // Увеличиваем таймаут до 10 секунд
        });
        console.log('C# application notified successfully');
        console.log(`Response status: ${response.status}`);
    } catch (error: any) {  // Утверждение типа 'error' как 'any'
        console.error('Error notifying C# app:', (error as Error).message);
        if (axios.isAxiosError(error)) {
            console.error('Axios error details:');
            if (error.response) {
                console.error('Response data:', error.response.data);
                console.error('Response status:', error.response.status);
                console.error('Response headers:', error.response.headers);
            } else if (error.request) {
                console.error('No response received from C# app. Request details:', error.request);
            } else {
                console.error('Error setting up request:', error.message);
            }
        } else {
            console.error('Unexpected error:', error);
        }
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
