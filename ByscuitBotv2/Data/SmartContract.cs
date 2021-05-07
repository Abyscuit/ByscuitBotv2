using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nethereum.Web3;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts.CQS;
using Nethereum.Util;
using Nethereum.Web3.Accounts;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Contracts;
using Nethereum.Contracts.Extensions;
using System.Numerics;

namespace ByscuitBotv2.Data
{
    public class SmartContract
    {

        [Function("totalSupply", "uint256")]
        public class TotalSupplyFunction : FunctionMessage
        {
        }

        [Function("balanceOf", "uint256")]
        public class BalanceOfFunction : FunctionMessage
        {
            [Parameter("address", "account", 1)]
            public string Owner { get; set; }
        }

        [Function("transfer", "bool")]
        public class TransferFunction : FunctionMessage
        {
            [Parameter("address", "recipient", 1)]
            public string To { get; set; }

            [Parameter("uint256", "amount", 2)]
            public BigInteger TokenAmount { get; set; }
        }

        [Event("Transfer")]
        public class TransferEventDTO : IEventDTO
        {
            [Parameter("address", "from", 1, true)]
            public string From { get; set; }

            [Parameter("address", "to", 2, true)]
            public string To { get; set; }

            [Parameter("uint256", "value", 3, false)]
            public BigInteger Value { get; set; }
        }


    }
}
