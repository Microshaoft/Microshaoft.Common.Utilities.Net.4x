﻿#if NETCOREAPP2_X
namespace Microshaoft.WebApi.ModelBinders
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging.Abstractions;
    using Microsoft.Extensions.Primitives;
    using Newtonsoft.Json.Linq;
    using System.Threading.Tasks;
    using Microshaoft.Web;
    public class JTokenModelBinder : IModelBinder
    {
        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var request = bindingContext
                                    .HttpContext
                                    .Request;

            IConfiguration configuration =
                        (IConfiguration) bindingContext
                                            .HttpContext
                                            .RequestServices
                                            .GetService
                                                (
                                                    typeof(IConfiguration)
                                                );
            var jwtTokenName = configuration
                                    .GetSection("TokenName")
                                    .Value;
            var ok = false;
            JToken parameters = null;
            ok = HttpRequestHelper
                    .TryParseJTokenParameters
                        (
                            request
                            , out parameters
                            , out var secretJwtToken
                            , async (x) =>
                            {
                                var formCollectionModelBinder =
                                            new FormCollectionModelBinder(NullLoggerFactory.Instance);
                                await formCollectionModelBinder.BindModelAsync(bindingContext);
                                if (bindingContext.Result.IsModelSet)
                                {
                                    x = JTokenWebHelper
                                                .ToJToken
                                                    (
                                                        (IFormCollection)
                                                            bindingContext
                                                                    .Result
                                                                    .Model
                                                    );
                                }
                            }
                            , jwtTokenName
                        );
            if (!StringValues.IsNullOrEmpty(secretJwtToken))
            {
                request
                    .HttpContext
                    .Items
                    .Add
                        (
                            jwtTokenName
                            , secretJwtToken
                        );
            }
            bindingContext
                    .Result =
                        ModelBindingResult
                                .Success
                                    (
                                        parameters
                                    );
        }
    }
    //public class JTokenModelBinderProvider
    //                        : IModelBinderProvider
    //                            , IModelBinder
    //{
    //    private IModelBinder _binder = new JTokenModelBinder();
    //    public async Task BindModelAsync(ModelBindingContext bindingContext)
    //    {
    //        await _binder.BindModelAsync(bindingContext);
    //    }
    //    public IModelBinder GetBinder(ModelBinderProviderContext context)
    //    {
    //        if (context == null)
    //        {
    //            throw new ArgumentNullException(nameof(context));
    //        }
    //        if (context.Metadata.ModelType == typeof(JToken))
    //        {
    //            //_binder = new JTokenModelBinder();
    //            return _binder;
    //        }
    //        return null;
    //    }
    //}
}
#endif