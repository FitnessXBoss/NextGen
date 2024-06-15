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
const express_1 = __importDefault(require("express"));
const ton_access_1 = require("@orbs-network/ton-access");
const crypto_1 = require("@ton/crypto");
const ton_1 = require("@ton/ton");
const uuid_1 = require("uuid");
const QRCode = __importStar(require("qrcode"));
const pg_1 = require("pg");
const axios_1 = __importDefault(require("axios"));
const core_1 = require("@ton/core");
const API_KEY = '6b347998e2359bc8039728754ac176830c60cde01bcad1170e1f058239bd4a33';
const pgClient = new pg_1.Client({
    host: '192.168.0.183',
    port: 5432,
    database: 'SecurityData',
    user: 'postgres',
    password: '72745621008',
});
pgClient.connect();
const app = (0, express_1.default)();
const port = 3000;
app.get('/generate-qrcode', (req, res) => __awaiter(void 0, void 0, void 0, function* () {
    const amount = req.query.amount;
    const uniqueId = (0, uuid_1.v4)();
    const mnemonic = "coral about client mandate inside shine inhale tumble royal garden crouch cook answer flight grape poverty inhale west spoil million stable exit shell elephant";
    const key = yield (0, crypto_1.mnemonicToWalletKey)(mnemonic.split(" "));
    const wallet = ton_1.WalletContractV4.create({ publicKey: key.publicKey, workchain: 0 });
    const expectedReturnAmount = (0, core_1.toNano)(parseFloat(amount)); // �������������� ����� � �����
    const tonLink = generateTonLink(wallet.address.toString(), expectedReturnAmount, uniqueId);
    yield generateQRCode(tonLink);
    res.send(`http://localhost:3000/qrcode.png`);
}));
app.listen(port, () => {
    console.log(`Server running at http://localhost:${port}/`);
});
function main() {
    return __awaiter(this, void 0, void 0, function* () {
        // Generate a unique value
        const uniqueId = (0, uuid_1.v4)();
        console.log(`Unique ID: ${uniqueId}`);
        // Open wallet v4 (ensure correct wallet version)
        const mnemonic = "coral about client mandate inside shine inhale tumble royal garden crouch cook answer flight grape poverty inhale west spoil million stable exit shell elephant";
        const key = yield (0, crypto_1.mnemonicToWalletKey)(mnemonic.split(" "));
        const wallet = ton_1.WalletContractV4.create({ publicKey: key.publicKey, workchain: 0 });
        // Initialize ton rpc client on the test network
        const endpoint = yield (0, ton_access_1.getHttpEndpoint)({ network: "testnet" });
        const client = new ton_1.TonClient({ endpoint });
        const contract = client.open(wallet);
        console.log(`Wallet address: ${wallet.address.toString()}`);
        // Ensure the wallet is deployed
        if (!(yield client.isContractDeployed(wallet.address))) {
            return console.log("Wallet is not deployed");
        }
        // Check wallet balance
        const balanceBefore = yield contract.getBalance();
        console.log(`Wallet balance before transfer: ${(0, core_1.fromNano)(balanceBefore)} TON`);
        const expectedReturnAmount = (0, core_1.toNano)(1); // 1 TON expected return amount from user
        // Generate QR code
        const tonLink = generateTonLink(wallet.address.toString(), expectedReturnAmount, uniqueId);
        yield generateQRCode(tonLink);
        // Wait for user transfer
        console.log(`Awaiting transfer from user to address: ${wallet.address.toString()} with amount: ${(0, core_1.fromNano)(expectedReturnAmount)} TON with comment: ${uniqueId}`);
        let received = false;
        while (!received) {
            try {
                const transactions = yield getTransactions(wallet.address.toString());
                for (const transaction of transactions) {
                    const transactionAmount = BigInt(transaction.value);
                    const comment = transaction.comment;
                    if (transactionAmount >= expectedReturnAmount && comment === uniqueId) {
                        received = true;
                        console.log(`Received amount: ${(0, core_1.fromNano)(transactionAmount)} TON, comment: ${comment}`);
                        console.log("Comment matches unique ID and amount is correct: TRUE");
                        console.log("Car sold");
                        const viewLink = `https://testnet.toncenter.com/api/v2/getTransactions?address=${wallet.address.toString()}&limit=1`;
                        // Record data in the database
                        yield pgClient.query('INSERT INTO payments (address, amount, comment, sender_wallet, transaction_id, timestamp, view_link) VALUES ($1, $2, $3, $4, $5, $6, $7)', [wallet.address.toString(), (0, core_1.fromNano)(transactionAmount), comment, transaction.sender, transaction.transaction_id.hash, new Date(transaction.utime * 1000), viewLink]);
                        console.log("Transaction successfully recorded in the database.");
                        // Notify C# application
                        yield notifyCSharpApp(transaction);
                        process.exit(0); // Exit the program
                    }
                }
                if (!received) {
                    yield dynamicSleep(2000); // Check every 2 seconds with a countdown
                }
            }
            catch (error) {
                console.error('Error fetching transaction details:', error);
                yield dynamicSleep(2000); // Check every 2 seconds with a countdown
            }
        }
    });
}
main();
function generateTonLink(address, amount, comment) {
    const amountInNano = amount.toString();
    return `ton://transfer/${address}?amount=${amountInNano}&text=${comment}`;
}
function generateQRCode(text) {
    return __awaiter(this, void 0, void 0, function* () {
        try {
            yield QRCode.toFile('qrcode.png', text);
            console.log('QR code saved as qrcode.png');
        }
        catch (err) {
            console.error('Error generating QR code', err);
        }
    });
}
function sleep(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
}
function dynamicSleep(ms) {
    return __awaiter(this, void 0, void 0, function* () {
        for (let remaining = ms; remaining > 0; remaining -= 1000) {
            process.stdout.write(`\rNext check in ${remaining / 1000} seconds...`);
            yield sleep(1000);
        }
        process.stdout.write('\r                              \r'); // Clear the line after the timer finishes
    });
}
function getTransactions(walletAddress) {
    return __awaiter(this, void 0, void 0, function* () {
        const apiEndpoint = 'https://testnet.toncenter.com/api/v2/getTransactions';
        const url = `${apiEndpoint}?address=${walletAddress}&limit=1`; // Check one transaction
        for (let attempt = 0; attempt < 3; attempt++) {
            try {
                const response = yield axios_1.default.get(url, {
                    headers: {
                        'Content-Type': 'application/json',
                        'Authorization': `Bearer ${API_KEY}` // Add API key in the header
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
function notifyCSharpApp(transaction) {
    return __awaiter(this, void 0, void 0, function* () {
        try {
            yield axios_1.default.post('http://localhost:5220/api/example/paymentSuccessful', {
                Comment: transaction.comment,
                Amount: transaction.value,
                Sender: transaction.sender,
            });
        }
        catch (error) {
            console.error('Error notifying C# app:', error);
        }
    });
}
