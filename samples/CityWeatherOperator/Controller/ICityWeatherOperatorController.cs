using CityWeatherOperator.Crd;
using System.Threading.Tasks;

namespace CityWeatherOperator.Controller
{
    public interface ICityWeatherOperatorController
    {
        Task Do(CityWeatherResource resource);

        Task UnDo(CityWeatherResource resource);
    }
}
