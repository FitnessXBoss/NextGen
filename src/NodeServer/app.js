"use strict";
var __createBinding = (this && this.__createBinding) || (Object.create ? (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    var desc = Object.getOwnPropertyDescriptor(m, k);
    if (!desc || ("get" in desc ? !m.__esModule : desc.writable || desc.configurable)) {
      desc = { enumerable: true, get: function() { return m[k]; } };
    }
    Object.defineProperty(o, k2, desc);
}) : (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    o[k2] = m[k];
}));
var __setModuleDefault = (this && this.__setModuleDefault) || (Object.create ? (function(o, v) {
    Object.defineProperty(o, "default", { enumerable: true, value: v });
}) : function(o, v) {
    o["default"] = v;
});
var __importStar = (this && this.__importStar) || function (mod) {
    if (mod && mod.__esModule) return mod;
    var result = {};
    if (mod != null) for (var k in mod) if (k !== "default" && Object.prototype.hasOwnProperty.call(mod, k)) __createBinding(result, mod, k);
    __setModuleDefault(result, mod);
    return result;
};
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.deleteQRCodeAfterUse = exports.handleTransaction = exports.sendTransactionData = exports.generateQRCode = exports.main = void 0;
const ton_access_1 = require("@orbs-network/ton-access");
const crypto_1 = require("@ton/crypto");
const ton_1 = require("@ton/ton");
const uuid_1 = require("uuid");
const QRCode = __importStar(require("qrcode"));
const fs = __importStar(require("fs"));
const pg_1 = require("pg");
const prompt_sync_1 = __importDefault(require("prompt-sync")); // ���������� ������
const path = __importStar(require("path"));
const core_1 = require("@ton/core");
const axios_1 = __importDefault(require("axios"));
const API_KEY = '6b347998e2359bc8039728754ac176830c60cde01bcad1170e1f058239bd4a33';
const pgClient = new pg_1.Client({
    host: '192.168.0.183',
    port: 5432,
    database: 'SecurityData',
    user: 'postgres',
    password: '72745621008',
});
function connectToDatabase() {
    return __awaiter(this, void 0, void 0, function* () {
        try {
            yield pgClient.connect();
            console.log('Connected to the database');
        }
        catch (err) {
            console.error('Failed to connect to the database. Retrying in 5 seconds...', err);
            setTimeout(connectToDatabase, 5000);
        }
    });
}
connectToDatabase();
pgClient.on('error', (err) => {
    console.error('Unexpected error on the database client:', err);
    connectToDatabase();
});
function main() {
    return __awaiter(this, void 0, void 0, function* () {
        const prompt = (0, prompt_sync_1.default)();
        // Generating a unique value
        const uniqueId = (0, uuid_1.v4)();
        console.log(`Unique value: ${uniqueId}`);
        // Opening Wallet v4 (pay attention to the correct wallet version)
        const mnemonic = "coral about client mandate inside shine inhale tumble royal garden crouch cook answer flight grape poverty inhale west spoil million stable exit shell elephant"; // your 24 secret words (replace ... with other words)
        const key = yield (0, crypto_1.mnemonicToWalletKey)(mnemonic.split(" "));
        const wallet = ton_1.WalletContractV4.create({ publicKey: key.publicKey, workchain: 0 });
        // Initializing TON RPC client on the test network
        const endpoint = yield (0, ton_access_1.getHttpEndpoint)({ network: "testnet" });
        const client = new ton_1.TonClient({ endpoint });
        const contract = client.open(wallet);
        console.log(`Wallet address: ${wallet.address.toString()}`);
        // Make sure the wallet is deployed
        if (!(yield client.isContractDeployed(wallet.address))) {
            return console.log("Wallet is not deployed");
        }
        // Checking the wallet balance
        const balanceBefore = yield contract.getBalance();
        console.log(`Wallet balance before the transfer: ${(0, core_1.fromNano)(balanceBefore)} TON`);
        const expectedReturnAmount = (0, core_1.toNano)(1); // 1 TON expected return amount from the user
        // Generating QR code
        const tonLink = generateTonLink(wallet.address.toString(), expectedReturnAmount, uniqueId);
        yield generateQRCode(wallet.address.toString(), (0, core_1.fromNano)(expectedReturnAmount).toString(), uniqueId, path.join(__dirname, 'uploads', 'qrcode_with_text.png'));
        // Waiting for the user's transfer
        console.log(`Waiting for the transfer to address: ${wallet.address.toString()} with the amount: ${(0, core_1.fromNano)(expectedReturnAmount)} TON and comment: ${uniqueId}`);
        let received = false;
        while (!received) {
            try {
                const transactions = yield getTransactions(wallet.address.toString());
                for (const transaction of transactions) {
                    const transactionAmount = BigInt(transaction.value);
                    const comment = transaction.comment;
                    const senderWallet = transaction.sender;
                    const transactionId = transaction.transaction_id;
                    const utime = transaction.utime;
                    const timestamp = new Date(utime * 1000); // Convert utime to Date
                    if (transactionAmount >= expectedReturnAmount && comment === uniqueId) {
                        received = true;
                        console.log(`Received amount: ${(0, core_1.fromNano)(transactionAmount)} TON, comment: ${comment}`);
                        console.log("Comment matches the unique value and the amount is correct: TRUE");
                        console.log("Transaction completed");
                        const viewLink = `https://testnet.toncenter.com/api/v2/getTransactions?address=${wallet.address.toString()}&limit=1`;
                        // Recording data in the database
                        yield pgClient.query('INSERT INTO payments (address, amount, comment, sender_wallet, transaction_id, timestamp, view_link) VALUES ($1, $2, $3, $4, $5, $6, $7)', [wallet.address.toString(), (0, core_1.fromNano)(transactionAmount), comment, senderWallet, transactionId.hash, timestamp, viewLink]);
                        console.log("Transaction successfully recorded in the database.");
                        // Deleting QR code after the transaction is completed
                        fs.unlinkSync(path.join(__dirname, 'uploads', 'qrcode_with_text.png'));
                        console.log("QR code deleted.");
                        // Sending data to C# API
                        yield handleTransaction(transaction);
                        process.exit(0); // Exiting the program
                    }
                }
                if (!received) {
                    console.log("Transaction not found or does not meet the conditions. Waiting...");
                    yield dynamicSleep(2000); // Check every 2 seconds
                }
            }
            catch (error) {
                console.error('Error fetching transaction details:', error);
                yield dynamicSleep(2000); // Check every 2 seconds
            }
        }
    });
}
exports.main = main;
function generateTonLink(address, amount, comment) {
    const amountInNano = amount.toString();
    return `ton://transfer/${address}?amount=${amountInNano}&text=${comment}`;
}
function generateQRCode(walletAddress, amount, uniqueId, outputPath) {
    return __awaiter(this, void 0, void 0, function* () {
        try {
            const tonLink = generateTonLink(walletAddress, BigInt((0, core_1.toNano)(amount)), uniqueId);
            const qrImage = yield QRCode.toBuffer(tonLink);
            fs.writeFileSync(outputPath, qrImage);
            console.log('QR code saved as qrcode_with_text.png');
        }
        catch (err) {
            console.error('Error generating QR code:', err);
        }
    });
}
exports.generateQRCode = generateQRCode;
function sleep(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
}
function dynamicSleep(ms) {
    return __awaiter(this, void 0, void 0, function* () {
        for (let remaining = ms; remaining > 0; remaining -= 1000) {
            process.stdout.write(`\rNext check in ${remaining / 1000} seconds...`);
            yield sleep(1000);
        }
        process.stdout.write('\r                              \r'); // Clearing the line after the timer ends
    });
}
function getTransactions(walletAddress) {
    return __awaiter(this, void 0, void 0, function* () {
        const apiEndpoint = 'https://testnet.toncenter.com/api/v2/getTransactions';
        const url = `${apiEndpoint}?address=${walletAddress}&limit=1`; // Checking a single transaction
        for (let attempt = 0; attempt < 3; attempt++) {
            try {
                const response = yield axios_1.default.get(url, {
                    headers: {
                        'Content-Type': 'application/json',
                        'Authorization': `Bearer ${API_KEY}` // Adding the API key in the header
                    }
                });
                if (response.status !== 200) {
                    throw new Error(`Error: ${response.statusText}`);
                }
                const data = response.data;
                const transactions = data.result; // All transactions from the result array
                if (transactions && transactions.length > 0) {
                    return transactions.map((transaction) => {
                        var _a, _b, _c;
                        const value = ((_a = transaction.in_msg) === null || _a === void 0 ? void 0 : _a.value) || '0';
                        const comment = (_b = transaction.in_msg) === null || _b === void 0 ? void 0 : _b.message;
                        const sender = (_c = transaction.in_msg) === null || _c === void 0 ? void 0 : _c.source;
                        const transactionId = transaction.transaction_id;
                        const utime = transaction.utime;
                        return { value, comment, sender, transaction_id: transactionId, utime };
                    });
                }
                else {
                    return [];
                }
            }
            catch (error) {
                if (error instanceof Error) {
                    console.error('Error fetching transaction details:', error.message);
                }
                else {
                    console.error('Unexpected error:', error);
                }
                if (attempt < 2) {
                    yield sleep(5000); // Delay before retrying
                }
                else {
                    throw error;
                }
            }
        }
    });
}
// Function for sending transaction data to C# API
function sendTransactionData(transaction) {
    return __awaiter(this, void 0, void 0, function* () {
        try {
            const response = yield axios_1.default.post('http://localhost:5220/api/example/receiveTransaction', transaction);
            console.log('Response from API:', response.status, response.statusText);
        }
        catch (error) {
            console.error('Error sending transaction data to API:', error);
        }
    });
}
exports.sendTransactionData = sendTransactionData;
// Example usage of sendTransactionData function
// This part should be called after receiving transaction data
function handleTransaction(transaction) {
    return __awaiter(this, void 0, void 0, function* () {
        const transactionData = {
            Comment: transaction.comment,
            Amount: transaction.value,
            Sender: transaction.sender,
        };
        try {
            yield sendTransactionData(transactionData);
            deleteQRCodeAfterUse(); // Calling the function to delete the QR code after successfully sending data to the API
        }
        catch (error) {
            console.error('Error handling transaction:', error);
            // Possible actions to be taken here in case of an error sending data to the API
        }
    });
}
exports.handleTransaction = handleTransaction;
function deleteQRCodeAfterUse() {
    return __awaiter(this, void 0, void 0, function* () {
        // Correctly specify the path to the QR code image
        const qrCodePath = path.join(__dirname, 'uploads', 'qrcode_with_text.png');
        try {
            if (fs.existsSync(qrCodePath)) {
                fs.unlinkSync(qrCodePath);
                console.log('QR code successfully deleted.');
            }
            else {
                console.log('QR code file not found, skipping deletion.');
            }
            // Creating a file for notification in C#
            fs.writeFileSync(path.join(__dirname, 'uploads', 'qr_code_deleted.txt'), '');
            console.log('Notification file successfully created.');
        }
        catch (err) {
            console.error('Error deleting QR code:', err);
        }
    });
}
exports.deleteQRCodeAfterUse = deleteQRCodeAfterUse;
