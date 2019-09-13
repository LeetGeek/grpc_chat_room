using System.Threading.Tasks;
using Ice.Service.Collateral;

namespace Ice.Service.CollateralEngine
{
    public interface ICollateralEngineClient
    {
        bool Connect(string host, int port);
        void Disconnect();


        Task<HaircutResult> CalculateHaircut(Asset asset);

    }
}