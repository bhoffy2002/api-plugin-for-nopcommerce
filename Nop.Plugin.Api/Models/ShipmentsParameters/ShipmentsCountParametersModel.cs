using Nop.Plugin.Api.ModelBinders;

namespace Nop.Plugin.Api.Models.ShipmentsParameters
{
    using Microsoft.AspNetCore.Mvc;

    [ModelBinder(typeof(ParametersModelBinder<ShipmentsCountParametersModel>))]
    public class ShipmentsCountParametersModel : BaseShipmentsParametersModel
    {
        // Nothing special here, created just for clarity.
    }
}