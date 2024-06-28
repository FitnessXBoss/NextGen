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
var __generator = (this && this.__generator) || function (thisArg, body) {
    var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g;
    return g = { next: verb(0), "throw": verb(1), "return": verb(2) }, typeof Symbol === "function" && (g[Symbol.iterator] = function() { return this; }), g;
    function verb(n) { return function (v) { return step([n, v]); }; }
    function step(op) {
        if (f) throw new TypeError("Generator is already executing.");
        while (g && (g = 0, op[0] && (_ = 0)), _) try {
            if (f = 1, y && (t = op[0] & 2 ? y["return"] : op[0] ? y["throw"] || ((t = y["return"]) && t.call(y), 0) : y.next) && !(t = t.call(y, op[1])).done) return t;
            if (y = 0, t) op = [op[0] & 2, t.value];
            switch (op[0]) {
                case 0: case 1: t = op; break;
                case 4: _.label++; return { value: op[1], done: false };
                case 5: _.label++; y = op[1]; op = [0]; continue;
                case 7: op = _.ops.pop(); _.trys.pop(); continue;
                default:
                    if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                    if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                    if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                    if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                    if (t[2]) _.ops.pop();
                    _.trys.pop(); continue;
            }
            op = body.call(thisArg, _);
        } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
        if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
    }
};
Object.defineProperty(exports, "__esModule", { value: true });
var express_1 = require("express");
var ton_access_1 = require("@orbs-network/ton-access");
var crypto_1 = require("@ton/crypto");
var ton_1 = require("@ton/ton");
var uuid_1 = require("uuid");
var QRCode = require("qrcode");
var pg_1 = require("pg");
var axios_1 = require("axios");
var core_1 = require("@ton/core");
var API_KEY = '6b347998e2359bc8039728754ac176830c60cde01bcad1170e1f058239bd4a33';
var pgClient = new pg_1.Client({
    host: '192.168.0.183',
    port: 5432,
    database: 'SecurityData',
    user: 'postgres',
    password: '72745621008',
});
pgClient.connect();
var app = (0, express_1.default)();
var port = 3000;
app.get('/generate-qrcode', function (req, res) { return __awaiter(void 0, void 0, void 0, function () {
    var amount, uniqueId, mnemonic, key, wallet, expectedReturnAmount, tonLink;
    return __generator(this, function (_a) {
        switch (_a.label) {
            case 0:
                amount = req.query.amount;
                uniqueId = (0, uuid_1.v4)();
                mnemonic = "coral about client mandate inside shine inhale tumble royal garden crouch cook answer flight grape poverty inhale west spoil million stable exit shell elephant";
                return [4 /*yield*/, (0, crypto_1.mnemonicToWalletKey)(mnemonic.split(" "))];
            case 1:
                key = _a.sent();
                wallet = ton_1.WalletContractV4.create({ publicKey: key.publicKey, workchain: 0 });
                expectedReturnAmount = (0, core_1.toNano)(parseFloat(amount));
                tonLink = generateTonLink(wallet.address.toString(), expectedReturnAmount, uniqueId);
                return [4 /*yield*/, generateQRCode(tonLink)];
            case 2:
                _a.sent();
                res.send("http://localhost:3000/qrcode.png");
                return [2 /*return*/];
        }
    });
}); });
app.listen(port, function () {
    console.log("Server running at http://localhost:".concat(port, "/"));
});
function main() {
    return __awaiter(this, void 0, void 0, function () {
        var uniqueId, mnemonic, key, wallet, endpoint, client, contract, balanceBefore, expectedReturnAmount, tonLink, received, transactions, _i, transactions_1, transaction, transactionAmount, comment, viewLink, error_1;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    uniqueId = (0, uuid_1.v4)();
                    console.log("Unique ID: ".concat(uniqueId));
                    mnemonic = "coral about client mandate inside shine inhale tumble royal garden crouch cook answer flight grape poverty inhale west spoil million stable exit shell elephant";
                    return [4 /*yield*/, (0, crypto_1.mnemonicToWalletKey)(mnemonic.split(" "))];
                case 1:
                    key = _a.sent();
                    wallet = ton_1.WalletContractV4.create({ publicKey: key.publicKey, workchain: 0 });
                    return [4 /*yield*/, (0, ton_access_1.getHttpEndpoint)({ network: "testnet" })];
                case 2:
                    endpoint = _a.sent();
                    client = new ton_1.TonClient({ endpoint: endpoint });
                    contract = client.open(wallet);
                    console.log("Wallet address: ".concat(wallet.address.toString()));
                    return [4 /*yield*/, client.isContractDeployed(wallet.address)];
                case 3:
                    // Ensure the wallet is deployed
                    if (!(_a.sent())) {
                        return [2 /*return*/, console.log("Wallet is not deployed")];
                    }
                    return [4 /*yield*/, contract.getBalance()];
                case 4:
                    balanceBefore = _a.sent();
                    console.log("Wallet balance before transfer: ".concat((0, core_1.fromNano)(balanceBefore), " TON"));
                    expectedReturnAmount = (0, core_1.toNano)(1);
                    tonLink = generateTonLink(wallet.address.toString(), expectedReturnAmount, uniqueId);
                    return [4 /*yield*/, generateQRCode(tonLink)];
                case 5:
                    _a.sent();
                    // Wait for user transfer
                    console.log("Awaiting transfer from user to address: ".concat(wallet.address.toString(), " with amount: ").concat((0, core_1.fromNano)(expectedReturnAmount), " TON with comment: ").concat(uniqueId));
                    received = false;
                    _a.label = 6;
                case 6:
                    if (!!received) return [3 /*break*/, 19];
                    _a.label = 7;
                case 7:
                    _a.trys.push([7, 16, , 18]);
                    return [4 /*yield*/, getTransactions(wallet.address.toString())];
                case 8:
                    transactions = _a.sent();
                    _i = 0, transactions_1 = transactions;
                    _a.label = 9;
                case 9:
                    if (!(_i < transactions_1.length)) return [3 /*break*/, 13];
                    transaction = transactions_1[_i];
                    transactionAmount = BigInt(transaction.value);
                    comment = transaction.comment;
                    if (!(transactionAmount >= expectedReturnAmount && comment === uniqueId)) return [3 /*break*/, 12];
                    received = true;
                    console.log("Received amount: ".concat((0, core_1.fromNano)(transactionAmount), " TON, comment: ").concat(comment));
                    console.log("Comment matches unique ID and amount is correct: TRUE");
                    console.log("Car sold");
                    viewLink = "https://testnet.toncenter.com/api/v2/getTransactions?address=".concat(wallet.address.toString(), "&limit=1");
                    // Record data in the database
                    return [4 /*yield*/, pgClient.query('INSERT INTO payments (address, amount, comment, sender_wallet, transaction_id, timestamp, view_link) VALUES ($1, $2, $3, $4, $5, $6, $7)', [wallet.address.toString(), (0, core_1.fromNano)(transactionAmount), comment, transaction.sender, transaction.transaction_id.hash, new Date(transaction.utime * 1000), viewLink])];
                case 10:
                    // Record data in the database
                    _a.sent();
                    console.log("Transaction successfully recorded in the database.");
                    // Notify C# application
                    return [4 /*yield*/, notifyCSharpApp(transaction)];
                case 11:
                    // Notify C# application
                    _a.sent();
                    process.exit(0); // Exit the program
                    _a.label = 12;
                case 12:
                    _i++;
                    return [3 /*break*/, 9];
                case 13:
                    if (!!received) return [3 /*break*/, 15];
                    return [4 /*yield*/, dynamicSleep(2000)];
                case 14:
                    _a.sent(); // Check every 2 seconds with a countdown
                    _a.label = 15;
                case 15: return [3 /*break*/, 18];
                case 16:
                    error_1 = _a.sent();
                    console.error('Error fetching transaction details:', error_1);
                    return [4 /*yield*/, dynamicSleep(2000)];
                case 17:
                    _a.sent(); // Check every 2 seconds with a countdown
                    return [3 /*break*/, 18];
                case 18: return [3 /*break*/, 6];
                case 19: return [2 /*return*/];
            }
        });
    });
}
main();
function generateTonLink(address, amount, comment) {
    var amountInNano = amount.toString();
    return "ton://transfer/".concat(address, "?amount=").concat(amountInNano, "&text=").concat(comment);
}
function generateQRCode(text) {
    return __awaiter(this, void 0, void 0, function () {
        var err_1;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    _a.trys.push([0, 2, , 3]);
                    return [4 /*yield*/, QRCode.toFile('qrcode.png', text)];
                case 1:
                    _a.sent();
                    console.log('QR code saved as qrcode.png');
                    return [3 /*break*/, 3];
                case 2:
                    err_1 = _a.sent();
                    console.error('Error generating QR code', err_1);
                    return [3 /*break*/, 3];
                case 3: return [2 /*return*/];
            }
        });
    });
}
function sleep(ms) {
    return new Promise(function (resolve) { return setTimeout(resolve, ms); });
}
function dynamicSleep(ms) {
    return __awaiter(this, void 0, void 0, function () {
        var remaining;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    remaining = ms;
                    _a.label = 1;
                case 1:
                    if (!(remaining > 0)) return [3 /*break*/, 4];
                    process.stdout.write("\rNext check in ".concat(remaining / 1000, " seconds..."));
                    return [4 /*yield*/, sleep(1000)];
                case 2:
                    _a.sent();
                    _a.label = 3;
                case 3:
                    remaining -= 1000;
                    return [3 /*break*/, 1];
                case 4:
                    process.stdout.write('\r                              \r'); // Clear the line after the timer finishes
                    return [2 /*return*/];
            }
        });
    });
}
function getTransactions(walletAddress) {
    return __awaiter(this, void 0, void 0, function () {
        var apiEndpoint, url, attempt, response, data, transactions, error_2;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    apiEndpoint = 'https://testnet.toncenter.com/api/v2/getTransactions';
                    url = "".concat(apiEndpoint, "?address=").concat(walletAddress, "&limit=1");
                    attempt = 0;
                    _a.label = 1;
                case 1:
                    if (!(attempt < 3)) return [3 /*break*/, 9];
                    _a.label = 2;
                case 2:
                    _a.trys.push([2, 4, , 8]);
                    return [4 /*yield*/, axios_1.default.get(url, {
                            headers: {
                                'Content-Type': 'application/json',
                                'Authorization': "Bearer ".concat(API_KEY) // Add API key in the header
                            }
                        })];
                case 3:
                    response = _a.sent();
                    if (response.status !== 200) {
                        throw new Error("Error: ".concat(response.statusText));
                    }
                    data = response.data;
                    transactions = data.result;
                    if (transactions && transactions.length > 0) {
                        return [2 /*return*/, transactions.map(function (transaction) {
                                var _a, _b, _c;
                                var value = ((_a = transaction.in_msg) === null || _a === void 0 ? void 0 : _a.value) || '0';
                                var comment = (_b = transaction.in_msg) === null || _b === void 0 ? void 0 : _b.message;
                                var sender = (_c = transaction.in_msg) === null || _c === void 0 ? void 0 : _c.source;
                                var transactionId = transaction.transaction_id;
                                var utime = transaction.utime;
                                return { value: value, comment: comment, sender: sender, transaction_id: transactionId, utime: utime };
                            })];
                    }
                    else {
                        return [2 /*return*/, []];
                    }
                    return [3 /*break*/, 8];
                case 4:
                    error_2 = _a.sent();
                    if (error_2 instanceof Error) {
                        console.error('Error fetching transaction details:', error_2.message);
                    }
                    else {
                        console.error('Unexpected error:', error_2);
                    }
                    if (!(attempt < 2)) return [3 /*break*/, 6];
                    return [4 /*yield*/, sleep(5000)];
                case 5:
                    _a.sent(); // Delay before retrying
                    return [3 /*break*/, 7];
                case 6: throw error_2;
                case 7: return [3 /*break*/, 8];
                case 8:
                    attempt++;
                    return [3 /*break*/, 1];
                case 9: return [2 /*return*/];
            }
        });
    });
}
function notifyCSharpApp(transaction) {
    return __awaiter(this, void 0, void 0, function () {
        var error_3;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    _a.trys.push([0, 2, , 3]);
                    return [4 /*yield*/, axios_1.default.post('http://localhost:5220/api/example/paymentSuccessful', {
                            Comment: transaction.comment,
                            Amount: transaction.value,
                            Sender: transaction.sender,
                        })];
                case 1:
                    _a.sent();
                    return [3 /*break*/, 3];
                case 2:
                    error_3 = _a.sent();
                    console.error('Error notifying C# app:', error_3);
                    return [3 /*break*/, 3];
                case 3: return [2 /*return*/];
            }
        });
    });
}
