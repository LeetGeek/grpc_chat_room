using System.Threading.Tasks;
using Grpc.Core;
using Ice.Service.Collateral;

namespace Ice.Service.CollateralEngine
{
    public class CollateralEngineClient2 : ICollateralEngineClient
    {
        private Collateral.CollateralEngine.CollateralEngineClient _client;
        private bool _shutdown;

        public bool Connect(string host, int port)
        {
            return false;
        }

        public void Disconnect()
        {
            _shutdown = true;
        }

        public async Task<HaircutResult> CalculateHaircut(Asset asset)
        {
            var value = await _client.CalculateHaircutAsync(new Asset());
            return value;
        }

        public async Task<HaircutResult> CalculateHaircut2(Asset asset)
        {
            using (var call = _client.Test2(new Asset()))
            {
                var responseStream = call.ResponseStream;

                while (await responseStream.MoveNext())
                {
                    var value = responseStream.Current;
                    call.Dispose();
                }
            }

            return null;
        }


    }
}