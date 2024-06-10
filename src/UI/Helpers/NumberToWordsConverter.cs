using System;

namespace NextGen.src.UI.Helpers
{
    public static class NumberToWordsConverter
    {
        private static readonly string[] Units =
        {
            "", "один", "два", "три", "четыре", "пять", "шесть", "семь", "восемь", "девять", "десять", "одиннадцать", "двенадцать", "тринадцать", "четырнадцать", "пятнадцать", "шестнадцать", "семнадцать", "восемнадцать", "девятнадцать"
        };

        private static readonly string[] Tens =
        {
            "", "", "двадцать", "тридцать", "сорок", "пятьдесят", "шестьдесят", "семьдесят", "восемьдесят", "девяносто"
        };

        private static readonly string[] Hundreds =
        {
            "", "сто", "двести", "триста", "четыреста", "пятьсот", "шестьсот", "семьсот", "восемьсот", "девятьсот"
        };

        private static readonly string[] Thousands =
        {
            "", "одна тысяча", "две тысячи", "три тысячи", "четыре тысячи", "пять тысяч", "шесть тысяч", "семь тысяч", "восемь тысяч", "девять тысяч"
        };

        public static string Convert(decimal number)
        {
            if (number == 0)
                return "ноль рублей 00 копеек";

            var parts = number.ToString("0.00").Replace(",", ".").Split('.');
            var integerPart = ConvertToWords(long.Parse(parts[0]));
            var fractionalPart = parts.Length > 1 ? parts[1] : "00";

            return $"{integerPart} рублей {fractionalPart} копеек";
        }

        private static string ConvertToWords(long number)
        {
            if (number == 0)
                return string.Empty;

            if (number < 20)
                return Units[number];

            if (number < 100)
                return Tens[number / 10] + " " + Units[number % 10];

            if (number < 1000)
                return Hundreds[number / 100] + " " + ConvertToWords(number % 100);

            if (number < 10000)
                return Thousands[number / 1000] + " " + ConvertToWords(number % 1000);

            if (number < 1000000)
                return ConvertToWords(number / 1000) + " тысяч " + ConvertToWords(number % 1000);

            if (number < 1000000000)
                return ConvertToWords(number / 1000000) + " миллионов " + ConvertToWords(number % 1000000);

            return ConvertToWords(number / 1000000000) + " миллиардов " + ConvertToWords(number % 1000000000);
        }
    }
}
