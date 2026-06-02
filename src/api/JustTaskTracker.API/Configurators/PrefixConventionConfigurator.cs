using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace JustTaskTracker.API.Configurators;

public class PrefixConventionConfigurator(string prefix) : IApplicationModelConvention
{
    private readonly AttributeRouteModel _prefixRoute = new(new RouteAttribute(prefix));

    public void Apply(ApplicationModel application)
    {
        foreach (var controller in application.Controllers)
        {
            foreach (var selector in controller.Selectors)
            {
                if (selector.AttributeRouteModel != null)
                {
                    selector.AttributeRouteModel = AttributeRouteModel.CombineAttributeRouteModel(
                        _prefixRoute,
                        selector.AttributeRouteModel);
                }
                else
                {
                    // If there is no [Route] attribute on the controller, create a new model instance
                    // to ensure that controllers do not share the same object reference in memory.
                    selector.AttributeRouteModel = new AttributeRouteModel(_prefixRoute);
                }
            }
        }
    }
}
