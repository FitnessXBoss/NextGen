"use strict";
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
const crypto_1 = require("@ton/crypto");
const ton_1 = require("@ton/ton");
const core_1 = require("@ton/core");
const uuid_1 = require("uuid");
const axios_1 = __importDefault(require("axios"));
const app = (0, express_1.default)();
const port = 3001;
// ���������� ���������� ��� �������� ������ � �����������
let generatedAddress = '';
let generatedComment = '';
let expectedReturnAmount = BigInt(0);
const API_KEY = '6b347998e2359bc8039728754ac176830c60cde01bcad1170e1f058239bd4a33';
app.use(express_1.default.json());
app.post('/generate-payment', (req, res) => __awaiter(void 0, void 0, void 0, function* () {
    const { amount } = req.body;
    const uniqueId = (0, uuid_1.v4)();
    const mnemonic = "coral about client mandate inside shine inhale tumble royal garden crouch cook answer flight grape poverty inhale west spoil million stable exit shell elephant";
    const key = yield (0, crypto_1.mnemonicToWalletKey)(mnemonic.split(" "));
    const wallet = ton_1.WalletContractV4.create({ publicKey: key.publicKey, workchain: 0 });
    expectedReturnAmount = (0, core_1.toNano)(parseFloat(amount)); // �������������� ����� � �����
    const tonLink = generateTonLink(wallet.address.toString(), expectedReturnAmount, uniqueId);
    // ��������� ����� � ����������� ��� ����������� ��������
    generatedAddress = wallet.address.toString();
    generatedComment = uniqueId;
    // ������� ����������� � �����
    console.log(`Expected comment: ${generatedComment}`);
    console.log(`Expected amount: ${(0, core_1.fromNano)(expectedReturnAmount)} TON`);
    res.json({
        address: wallet.address.toString(),
        uniqueId,
        amount: expectedReturnAmount.toString(),
        tonLink
    });
    // ��������� ������� �������� ����������
    checkForTransactions();
}));
function checkForTransactions() {
    return __awaiter(this, void 0, void 0, function* () {
        let received = false;
        while (!received) {
            try {
                const transactions = yield getTransactions(generatedAddress);
                for (const transaction of transactions) {
                    const transactionAmount = BigInt(transaction.value);
                    const comment = transaction.comment;
                    if (transactionAmount >= expectedReturnAmount && comment === generatedComment) {
                        received = true;
                        console.log(`Received amount: ${(0, core_1.fromNano)(transactionAmount)} TON, comment: ${comment}`);
                        console.log("Comment matches unique ID and amount is correct: TRUE");
                        console.log("Payment verified successfully");
                        // �������������� ��������, ��������, ����������� C# ����������
                        yield notifyCSharpApp(transaction);
                        break;
                    }
                }
                if (!received) {
                    yield dynamicSleep(2000); // ��������� ������ 2 �������
                }
            }
            catch (error) {
                if (error instanceof Error) {
                    console.error('Error fetching transaction details:', error.message);
                }
                else {
                    console.error('Unexpected error:', error);
                }
                yield dynamicSleep(2000); // ��������� ������ 2 �������
            }
        }
    });
}
app.listen(port, () => {
    console.log(`Server running at http://localhost:${port}/`);
});
function generateTonLink(address, amount, comment) {
    const amountInNano = amount.toString();
    return `ton://transfer/${address}?amount=${amountInNano}&text=${comment}`;
}
function getTransactions(walletAddress) {
    return __awaiter(this, void 0, void 0, function* () {
        const apiEndpoint = 'https://testnet.toncenter.com/api/v2/getTransactions';
        const url = `${apiEndpoint}?address=${walletAddress}&limit=5`; // �������� ���������� ��������� ����������
        try {
            const response = yield axios_1.default.get(url, {
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${API_KEY}`
                }
            });
            const data = response.data;
            const transactions = data.result;
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
        catch (error) {
            if (error instanceof Error) {
                console.error('Error fetching transaction details:', error.message);
            }
            else {
                console.error('Unexpected error:', error);
            }
            throw error;
        }
    });
}
function notifyCSharpApp(transaction) {
    return __awaiter(this, void 0, void 0, function* () {
        try {
            const response = yield axios_1.default.post('http://localhost:5220/api/example/paymentSuccessful', {
                Comment: transaction.comment,
                Amount: transaction.value, // value � nanoTON
                Sender: transaction.sender,
            });
            console.log('C# application notified successfully');
            console.log(`Response status: ${response.status}`);
        }
        catch (error) {
            if (axios_1.default.isAxiosError(error)) {
                console.error('Error notifying C# app:', error.message);
                if (error.response) {
                    console.error('Response data:', error.response.data);
                    console.error('Response status:', error.response.status);
                    console.error('Response headers:', error.response.headers);
                }
            }
            else {
                console.error('Unexpected error:', error);
            }
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
