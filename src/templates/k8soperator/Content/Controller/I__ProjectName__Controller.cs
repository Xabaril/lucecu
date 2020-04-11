using __ProjectName__.Crd;
using System.Threading.Tasks;

namespace __ProjectName__.Controller
{
    public interface I__ProjectName__Controller
    {
        ValueTask Do({{crdname}}Resource resource);

        ValueTask UnDo({{crdname}}Resource resource);
    }
}
