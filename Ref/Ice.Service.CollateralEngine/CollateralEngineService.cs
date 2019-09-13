using System.Threading.Tasks;
using Grpc.Core;
using Ice.Service.Collateral;

namespace Ice.Service.CollateralEngine
{
    public class CollateralEngineService : Collateral.CollateralEngine.CollateralEngineBase
    {
        public override async Task<HaircutResult> CalculateHaircut(Asset request, ServerCallContext context)
        {
            var result = await Task.FromResult(new HaircutResult());

            return result;
        }
    }
}