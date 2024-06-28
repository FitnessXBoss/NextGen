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
var crypto_1 = require("@ton/crypto");
var ton_1 = require("@ton/ton");
var core_1 = require("@ton/core");
var uuid_1 = require("uuid");
var axios_1 = require("axios");
var app = (0, express_1.default)();
var port = 3001;
var generatedAddress = '';
var generatedComment = '';
var expectedReturnAmount = BigInt(0);
var paymentReceived = false;
var API_KEY = '6b347998e2359bc8039728754ac176830c60cde01bcad1170e1f058239bd4a33';
app.use(express_1.default.json());
app.post('/generate-payment', function (req, res) { return __awaiter(void 0, void 0, void 0, function () {
    var amount, uniqueId, mnemonic, key, wallet, tonLink;
    return __generator(this, function (_a) {
        switch (_a.label) {
            case 0:
                amount = req.body.amount;
                uniqueId = (0, uuid_1.v4)();
                mnemonic = "coral about client mandate inside shine inhale tumble royal garden crouch cook answer flight grape poverty inhale west spoil million stable exit shell elephant";
                return [4 /*yield*/, (0, crypto_1.mnemonicToWalletKey)(mnemonic.split(" "))];
            case 1:
                key = _a.sent();
                wallet = ton_1.WalletContractV4.create({ publicKey: key.publicKey, workchain: 0 });
                expectedReturnAmount = (0, core_1.toNano)(parseFloat(amount));
                tonLink = generateTonLink(wallet.address.toString(), expectedReturnAmount, uniqueId);
                generatedAddress = wallet.address.toString();
                generatedComment = uniqueId;
                console.log("Expected comment: ".concat(generatedComment));
                console.log("Expected amount: ".concat((0, core_1.fromNano)(expectedReturnAmount), " TON"));
                res.json({
                    address: wallet.address.toString(),
                    uniqueId: uniqueId,
                    amount: expectedReturnAmount.toString(),
                    tonLink: tonLink
                });
                checkForTransactions();
                return [2 /*return*/];
        }
    });
}); });
app.get('/checkStatus', function (req, res) {
    res.json({ paymentReceived: paymentReceived });
});
function checkForTransactions() {
    return __awaiter(this, void 0, void 0, function () {
        var received, transactions, _i, transactions_1, transaction, transactionAmount, comment, error_1;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    received = false;
                    _a.label = 1;
                case 1:
                    if (!!received) return [3 /*break*/, 13];
                    _a.label = 2;
                case 2:
                    _a.trys.push([2, 10, , 12]);
                    return [4 /*yield*/, getTransactions(generatedAddress)];
                case 3:
                    transactions = _a.sent();
                    _i = 0, transactions_1 = transactions;
                    _a.label = 4;
                case 4:
                    if (!(_i < transactions_1.length)) return [3 /*break*/, 7];
                    transaction = transactions_1[_i];
                    transactionAmount = BigInt(transaction.value);
                    comment = transaction.comment;
                    if (!(transactionAmount >= expectedReturnAmount && comment === generatedComment)) return [3 /*break*/, 6];
                    received = true;
                    paymentReceived = true;
                    console.log("Received amount: ".concat((0, core_1.fromNano)(transactionAmount), " TON, comment: ").concat(comment));
                    console.log("Comment matches unique ID and amount is correct: TRUE");
                    console.log("Payment verified successfully");
                    return [4 /*yield*/, notifyCSharpApp(transaction)];
                case 5:
                    _a.sent();
                    return [3 /*break*/, 7];
                case 6:
                    _i++;
                    return [3 /*break*/, 4];
                case 7:
                    if (!!received) return [3 /*break*/, 9];
                    return [4 /*yield*/, dynamicSleep(2000)];
                case 8:
                    _a.sent();
                    _a.label = 9;
                case 9: return [3 /*break*/, 12];
                case 10:
                    error_1 = _a.sent();
                    if (error_1 instanceof Error) {
                        console.error('Error fetching transaction details:', error_1.message);
                    }
                    else {
                        console.error('Unexpected error:', error_1);
                    }
                    return [4 /*yield*/, dynamicSleep(2000)];
                case 11:
                    _a.sent();
                    return [3 /*break*/, 12];
                case 12: return [3 /*break*/, 1];
                case 13: return [2 /*return*/];
            }
        });
    });
}
app.listen(port, function () {
    console.log("Server running at http://localhost:".concat(port, "/"));
});
function generateTonLink(address, amount, comment) {
    var amountInNano = amount.toString();
    return "ton://transfer/".concat(address, "?amount=").concat(amountInNano, "&text=").concat(comment);
}
function getTransactions(walletAddress) {
    return __awaiter(this, void 0, void 0, function () {
        var apiEndpoint, url, response, data, transactions, error_2;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    apiEndpoint = 'https://testnet.toncenter.com/api/v2/getTransactions';
                    url = "".concat(apiEndpoint, "?address=").concat(walletAddress, "&limit=5");
                    _a.label = 1;
                case 1:
                    _a.trys.push([1, 3, , 4]);
                    return [4 /*yield*/, axios_1.default.get(url, {
                            headers: {
                                'Content-Type': 'application/json',
                                'Authorization': "Bearer ".concat(API_KEY)
                            }
                        })];
                case 2:
                    response = _a.sent();
                    data = response.data;
                    transactions = data.result;
                    return [2 /*return*/, transactions.map(function (transaction) {
                            var _a, _b, _c;
                            var value = ((_a = transaction.in_msg) === null || _a === void 0 ? void 0 : _a.value) || '0';
                            var comment = (_b = transaction.in_msg) === null || _b === void 0 ? void 0 : _b.message;
                            var sender = (_c = transaction.in_msg) === null || _c === void 0 ? void 0 : _c.source;
                            var transactionId = transaction.transaction_id;
                            var utime = transaction.utime;
                            return { value: value, comment: comment, sender: sender, transaction_id: transactionId, utime: utime };
                        })];
                case 3:
                    error_2 = _a.sent();
                    if (axios_1.default.isAxiosError(error_2)) {
                        console.error('Axios error while fetching transactions:', error_2.message);
                        if (error_2.response) {
                            console.error('Response data:', error_2.response.data);
                            console.error('Response status:', error_2.response.status);
                            console.error('Response headers:', error_2.response.headers);
                        }
                        else if (error_2.request) {
                            console.error('No response received from TON Center. Request details:', error_2.request);
                        }
                    }
                    else {
                        console.error('Unexpected error:', error_2);
                    }
                    throw error_2;
                case 4: return [2 /*return*/];
            }
        });
    });
}
function notifyCSharpApp(transaction) {
    return __awaiter(this, void 0, void 0, function () {
        var response, error_3;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    console.log('Notifying C# app with transaction data:', transaction);
                    _a.label = 1;
                case 1:
                    _a.trys.push([1, 3, , 4]);
                    return [4 /*yield*/, axios_1.default.post('http://localhost:5220/api/payment/paymentSuccessful', {
                            Comment: transaction.comment,
                            Amount: transaction.value,
                            Sender: transaction.sender,
                        }, {
                            timeout: 10000
                        })];
                case 2:
                    response = _a.sent();
                    console.log('C# application notified successfully');
                    console.log("Response status: ".concat(response.status));
                    return [3 /*break*/, 4];
                case 3:
                    error_3 = _a.sent();
                    console.error('Error notifying C# app:', error_3.message);
                    if (axios_1.default.isAxiosError(error_3)) {
                        console.error('Axios error details:');
                        if (error_3.response) {
                            console.error('Response data:', error_3.response.data);
                            console.error('Response status:', error_3.response.status);
                            console.error('Response headers:', error_3.response.headers);
                        }
                        else if (error_3.request) {
                            console.error('No response received from C# app. Request details:', error_3.request);
                        }
                        else {
                            console.error('Error setting up request:', error_3.message);
                        }
                    }
                    else {
                        console.error('Unexpected error:', error_3);
                    }
                    return [3 /*break*/, 4];
                case 4: return [2 /*return*/];
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
