using System;
using System.Numerics;
using Nethereum.Util;

namespace ElympicsPlayPad.Utility
{
    public static class WeiConverter
    {
        public static Decimal FromWei(BigInteger value, int decimalPlacesToUnit) => (Decimal) new BigDecimal(value, decimalPlacesToUnit * -1);
    }
}
