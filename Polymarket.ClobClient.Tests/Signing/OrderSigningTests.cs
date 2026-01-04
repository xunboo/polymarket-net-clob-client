using System;
using System.Numerics;
using System.Collections.Generic;
using Xunit;
using System.IO;
using Polymarket.ClobClient.Signing;
using Polymarket.ClobClient.Utilities;
using Polymarket.ClobClient.Models;
using Polymarket.ClobClient.Constants;
using Nethereum.Signer;

namespace Polymarket.ClobClient.Tests.Signing
{
    public class OrderSigningTests
    {
        private const string PrivateKey = "0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80";
        private const string Token = "71321045679252212594626385532706912750332728571942532289631379312455583992563";

        [Fact]
        public void SignOrder_AllCombinations_ShouldProduceValidSignedOrders()
        {
            var key = new EthECKey(PrivateKey);
            var address = key.GetPublicAddress();
            var chainId = (int)Models.Chain.Amoy;
            var exchange = Contracts.GetContractConfig(chainId).Exchange;

            var tickSizes = new[] { "0.1", "0.01" };
            var signatureTypes = new[] { 0, 1, 2 }; // EOA, POLY_PROXY, POLY_GNOSIS_SAFE
            var orderTypes = new[] { OrderType.Gtd, OrderType.Gtc, OrderType.Fok };
            var sides = new[] { Side.Buy, Side.Sell };

            var expectedSignatures = new Dictionary<string, string>
            {
                // tick 0.1
                { "0.1:Gtd:BUY:0", "0xc8d954f33df23b8a00d6e91096759becab3708f94c6d643c1160799592b114731cf7181526e27bdcc429fc670930f2036c01b0dc41e07c19b7f3191d37b123a61b" },
                { "0.1:Gtd:BUY:1", "0x883a2919b3650854720bd5ba6be23506fdafe408ec0a67fbc69b1a21d6b17c4d7030bfd22b123c0bd86bea9f33c85984e19c2b788821718795551ffb887893751c" },
                { "0.1:Gtd:BUY:2", "0xeccf427d72059be06f91d01331242100db227b8a3db6cae0ff4daba806a87cd53124e6234cfb8ba30ee6fdcbecd84e7bb569ee85f2095105aa20cac5a2ecf2e31c" },
                { "0.1:Gtd:SELL:0", "0x6c0ecacea995dbec22aff0fb03295f9a94accce3ccb21765df2a905b4c3e11f0670462fd7c9c409aa93590d41fa535bd7db466b310978d0b5d2d2eccbed780191c" },
                { "0.1:Gtd:SELL:1", "0x1a2e3a709c0be0d284977413337bae858a8cabdc5d31db286d9b5c138bf398f430342c16051140dd3e4d56dd45a4d8ad7ae7a26ae96a2ebe94c5bcb3459564fd1b" },
                { "0.1:Gtd:SELL:2", "0xbdce7598388ced24fc221f5f5570584d1f3ca78ebfdb9d92e95b7e5179fee49949b2d626dea4c9efe0fec8210f729fbd31c9ec03cee88e036d9ad8d6b8b059ee1c" },
                { "0.1:Gtc:BUY:0", "0xd0612c5256b6e37109c9d59337741aa07185b1e2de5d6dbad2c19b9c7dfda9db1dc58807197fba3b84c448c26a90aee874d7038207c107b4f1fd8ac7d762f26e1c" },
                { "0.1:Gtc:BUY:1", "0x97731b0dd857003112937d0c157959c38cb1866be5029da3f815831df0185795052894319b5989adb53f1099466d9af2720437523b9754d512f6edb51108cbc41b" },
                { "0.1:Gtc:BUY:2", "0x860e030b45dd0634639b9687141fd0beb41cb0c1337ce8e13aa4c57289d309e151eea526d37fa4e4fdf80187232ce23cd0792187b484a75a8956d7f091ac72c11c" },
                { "0.1:Gtc:SELL:0", "0x9d3ea42b2708a1de7d9d667a0aedcd87794dd5334034ee5d50f934addb62d0eb1cc67e954ea11c6d1e35ee060850115faffa610fb8525eac7b8ff64ec1e8c5331b" },
                { "0.1:Gtc:SELL:1", "0xed25af68d7315edb0b329a54163b2c3df52bdbce9a109711c03fc8271da815f568486448855fffb2747ad5a3f6c8869d0f5f74124fc013b6cee0acf96e86e0f51c" },
                { "0.1:Gtc:SELL:2", "0x1d4488bbb404389d6008fd07a7bf6dad1ab210e06e2e32a016871785d2527e0526e3cb13f1fd0e189a04bc3581e78af22cff4e3c8f62c7f753dac6d019ffca681b" },
                { "0.1:Fok:BUY:0", "0xc1814cf27b6cbce8cca938f09cd4f7e33eb394c54a0450ee1e6eeb90f91739d31c08a3cd5763f94188c21392951ddbc7bbf422b8594be502e1d77031c09a46a11b" },
                { "0.1:Fok:BUY:1", "0xa395821f4bd5efbeb4deae4db27e77d4504d865c2131a53398c7dab178966ed03eeeffa74a50758bd966256aeff4a5f16899fdb31b468f27732b0109e11d4dba1b" },
                { "0.1:Fok:BUY:2", "0x41bda7e65a029e224ae6c67c21e7a0e6f63c9756f35789515377db3037b03935396fc270d996d47cc10283c6f577b6479fae8d99896c14daed4a65de4b1399d51b" },
                { "0.1:Fok:SELL:0", "0x9d3ea42b2708a1de7d9d667a0aedcd87794dd5334034ee5d50f934addb62d0eb1cc67e954ea11c6d1e35ee060850115faffa610fb8525eac7b8ff64ec1e8c5331b" },
                { "0.1:Fok:SELL:1", "0xed25af68d7315edb0b329a54163b2c3df52bdbce9a109711c03fc8271da815f568486448855fffb2747ad5a3f6c8869d0f5f74124fc013b6cee0acf96e86e0f51c" },
                { "0.1:Fok:SELL:2", "0x1d4488bbb404389d6008fd07a7bf6dad1ab210e06e2e32a016871785d2527e0526e3cb13f1fd0e189a04bc3581e78af22cff4e3c8f62c7f753dac6d019ffca681b" },
                // tick 0.01
                { "0.01:Gtd:BUY:0", "0x8fc2277fec8ff564db6edf019bd043d7155c0c760d25ea92e7aa83ef0d0b1a5a73bbd678919c14b418a04a867f851afae1de069c762e6ae4b4e66f3f612f0afe1c" },
                { "0.01:Gtd:BUY:1", "0xd9b075178bd75503ef781a45d2ec944b88ac303f216ea011fe2aacb998e893713c489afec9add72cfb28fbbd601617da9318b3f6b4f8bc7a347140d16d3ee6dd1c" },
                { "0.01:Gtd:BUY:2", "0xb937a837a60c33fba70af0d08eef54067d456c669314e23509098627609d6bed4c032d0423cefd80c0282d034968552d6089af2b8c4cf0d339694a98ec737d661b" },
                { "0.01:Gtd:SELL:0", "0x41fa8fd393055d465d7e917612545a0f5ca700bf2329f0d313b7a727aab692ff7bbbeb3dc70d8d1b0f674a6e1ac23ef5443e097d65863baca2963c644cc0d12f1c" },
                { "0.01:Gtd:SELL:1", "0x8470c0fb8c1daab3188a6ee3a5c8a06b0190252959d0f97f98b0e4ca6131c0e049a4ccd521cb0fd5890621688958c81e928c1031b60e59b6b0f8d298ead3337e1c" },
                { "0.01:Gtd:SELL:2", "0x00f674428ef723b913f281899e1d5a17c54f5903001100c4ead04fec34324a255e517c0bbdf6bfe530a52029f84765e7b0655b0f364e68a739f839142a9fec621c" },
                { "0.01:Gtc:BUY:0", "0x12b65c89a96dd46d819422091ccbb29b5e77d7d5dfbc6800dcf35c8e53d012116a61b52b08170544b367517de6c912da42770365a05f508104e6f751ffe599331b" },
                { "0.01:Gtc:BUY:1", "0x4f32f4038b6035119e19ca69ed7f3d062cfb891a1f1c929e2ad65eceeab94c1240d9152ba95660cc18f49be9bc47140ad5c3a11d23c1d9a56cb682c8e6db102e1b" },
                { "0.01:Gtc:BUY:2", "0x5741b6e9450c7ea415a3b3fa67fe63a914bdee960146dfc1742c5cfe8d39baf66bcb38338374cf8e5706d414e231a3fd1719c64a097b1b4fc2316e13d8a4c94a1c" },
                { "0.01:Gtc:SELL:0", "0x4e2888798806a2262dde4274d5ea69d5a2a47f26f51e825345713f07a84599db452e16477bebd07a95382514d538edebd8c9dc3fafc43636f4a31666f6516a6a1c" },
                { "0.01:Gtc:SELL:1", "0xc430148e9aac91b73ba31d7deb126bd67fad535094d9409420e2e2f637dad2853c9e46c1230d2d5a66778cee90139fb0d3e79db6eb7757615fc9abecf8940ce21b" },
                { "0.01:Gtc:SELL:2", "0x59d7a1e6ff1e2b002581406dfd1d5961f97ecfa51faddc840eccab59ac5f568a2bd75d2001d9f0bf03e8c9a40b134b8eaff9cda2c1fe2ac5061a0d11700ae3981c" },
                { "0.01:Fok:BUY:0", "0x138308131938615adee8b9e0327e4709ddd66898f7198b0b9e02cc2d49eab99307139b6d7e59248d19170e71667b6948435658f6c7abaec309b1a02f35b474b81b" },
                { "0.01:Fok:BUY:1", "0x97fca2c0878372b5ca03c54bcf82e4df248eb12241db7cf1b7d63a87b0dcba737e2faa8509a7b0b8469970cdff702e2a50af968a2e6dc504878b62b308c5563d1b" },
                { "0.01:Fok:BUY:2", "0x1ee840665c90ae325de151161b39973d735a18c32358dc645a4d2345cd2a734c475627aff843293f64bdeb894ffa186c584c9fc5e149541aaaaa050d3099fa021b" },
                { "0.01:Fok:SELL:0", "0x4e2888798806a2262dde4274d5ea69d5a2a47f26f51e825345713f07a84599db452e16477bebd07a95382514d538edebd8c9dc3fafc43636f4a31666f6516a6a1c" },
                { "0.01:Fok:SELL:1", "0xc430148e9aac91b73ba31d7deb126bd67fad535094d9409420e2e2f637dad2853c9e46c1230d2d5a66778cee90139fb0d3e79db6eb7757615fc9abecf8940ce21b" },
                { "0.01:Fok:SELL:2", "0x59d7a1e6ff1e2b002581406dfd1d5961f97ecfa51faddc840eccab59ac5f568a2bd75d2001d9f0bf03e8c9a40b134b8eaff9cda2c1fe2ac5061a0d11700ae3981c" }
            };

            foreach (var tick in tickSizes)
            {
                var roundConfig = OrderUtils.RoundingConfig[tick];

                foreach (var orderType in orderTypes)
                {
                    foreach (var side in sides)
                    {
                        foreach (var sigType in signatureTypes)
                        {
                            // Prepare inputs
                            decimal price = tick == "0.1" ? 0.5m : 0.05m;
                            decimal sizeOrAmount = 100m;

                            decimal rawMaker = 0m;
                            decimal rawTaker = 0m;

                            if (orderType == OrderType.Fok)
                            {
                                var res = OrderUtils.GetMarketOrderRawAmounts(side, sizeOrAmount, price, roundConfig);
                                rawMaker = res.RawMakerAmt;
                                rawTaker = res.RawTakerAmt;
                            }
                            else
                            {
                                var res = OrderUtils.GetOrderRawAmounts(side, sizeOrAmount, price, roundConfig);
                                rawMaker = res.RawMakerAmt;
                                rawTaker = res.RawTakerAmt;
                            }

                            var makerAmount = OrderUtils.ParseUnits(rawMaker, OrderUtils.CollateralTokenDecimals);
                            var takerAmount = OrderUtils.ParseUnits(rawTaker, OrderUtils.CollateralTokenDecimals);

                            var salt = new BigInteger(1000); // deterministic salt for tests
                            var expiration = orderType == OrderType.Gtd ? new BigInteger(1709948026) : BigInteger.Zero;
                            var nonce = BigInteger.Zero;
                            var feeRateBps = BigInteger.Zero;

                            var orderStruct = new OrderStruct
                            {
                                Salt = salt,
                                Maker = address,
                                Signer = address,
                                Taker = "0x0000000000000000000000000000000000000000",
                                TokenId = BigInteger.Parse(Token),
                                MakerAmount = makerAmount,
                                TakerAmount = takerAmount,
                                Expiration = expiration,
                                Nonce = nonce,
                                FeeRateBps = feeRateBps,
                                Side = side == Side.Buy ? 0 : 1,
                                SignatureType = sigType
                            };

                            var signature = OrderSigner.SignOrder(orderStruct, PrivateKey, chainId, exchange);

                            var mapKey = $"{tick}:{orderType}:{(orderStruct.Side==0?"BUY":"SELL")}:{sigType}";
                            Assert.True(expectedSignatures.ContainsKey(mapKey), $"Missing expected signature for {mapKey}");
                            Assert.Equal(expectedSignatures[mapKey], signature);

                            // Basic assertions matching the TS expectations: field values present and consistent
                            Assert.Equal(address, orderStruct.Maker);
                            Assert.Equal(address, orderStruct.Signer);
                            Assert.Equal("0x0000000000000000000000000000000000000000", orderStruct.Taker);
                            Assert.Equal(makerAmount, orderStruct.MakerAmount);
                            Assert.Equal(takerAmount, orderStruct.TakerAmount);
                            Assert.Equal(sigType, orderStruct.SignatureType);
                            Assert.True(orderStruct.Side == 0 || orderStruct.Side == 1);
                            Assert.False(string.IsNullOrEmpty(signature));
                        }
                    }
                }
            }

            // exact signature asserts added above
        }
    }
}
