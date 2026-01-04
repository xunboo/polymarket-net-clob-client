using Xunit;
using Polymarket.ClobClient.Signing;

namespace Polymarket.ClobClient.Tests.Signing
{
    public class HmacTests
    {
        [Fact]
        public void BuildPolyHmacSignature_ShouldReturnCorrectSignature()
        {
            // From TS test:
            // buildPolyHmacSignature("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=", 1000000, "test-sign", "/orders", '{"hash": "0x123"}')
            // Expected: "ZwAdJKvoYRlEKDkNMwd5BuwNNtg93kNaR_oU2HrfVvc="
            
            var signature = SignerUtils.BuildPolyHmacSignature(
                "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=",
                1000000,
                "test-sign",
                "/orders",
                "{\"hash\": \"0x123\"}"
            );

            Assert.NotNull(signature);
            Assert.NotEmpty(signature);
            Assert.Equal("ZwAdJKvoYRlEKDkNMwd5BuwNNtg93kNaR_oU2HrfVvc=", signature);
        }

        [Fact]
        public void BuildPolyHmacSignature_ShouldTransformBase64UrlToBase64()
        {
            // Base64 secret with + and /
            var base64Signature = SignerUtils.BuildPolyHmacSignature(
                "++/AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=",
                1000000,
                "test-sign",
                "/orders",
                "{\"hash\": \"0x123\"}"
            );

            // Base64url secret with - and _
            var base64UrlSignature = SignerUtils.BuildPolyHmacSignature(
                "--_AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
                1000000,
                "test-sign",
                "/orders",
                "{\"hash\": \"0x123\"}"
            );

            Assert.Equal(base64UrlSignature, base64Signature);
        }

        [Fact]
        public void BuildPolyHmacSignature_ShouldIgnoreInvalidSymbols()
        {
            // TS test: invalid symbols in base64 are stripped for backwards compatibility
            var signature = SignerUtils.BuildPolyHmacSignature(
                "AAAAAAAAA^^AAAAAAAA<>AAAAA||AAAAAAAAAAAAAAAAAAAAA=",
                1000000,
                "test-sign",
                "/orders",
                "{\"hash\": \"0x123\"}"
            );

            Assert.NotNull(signature);
            Assert.NotEmpty(signature);
            // Expected to match the clean secret result
            Assert.Equal("ZwAdJKvoYRlEKDkNMwd5BuwNNtg93kNaR_oU2HrfVvc=", signature);
        }
    }
}
