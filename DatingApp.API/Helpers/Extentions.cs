using System;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DatingApp.API.Helpers
{
    public static class Extentions
    {
        public static void AddApplicationError(this HttpResponse response, string message){
            response.Headers.Add("Application-Error", message);
            response.Headers.Add("Access-Control-Expose-Headers", "Application-Error");
            response.Headers.Add("Access-Control-Allow-Origin", "*");
        }

        public static void AddPagination(this HttpResponse response, int currentPage, int itemsPerPage, int totalItems, int totalPages) {
            var paginationHeader = new PaginationHeader(currentPage, itemsPerPage, totalItems, totalPages);

            // formata retorno para ser 'titleCase' em vez de CamelCase
            var cameCaseFormatter = new JsonSerializerSettings();
            cameCaseFormatter.ContractResolver = new CamelCasePropertyNamesContractResolver();

            response.Headers.Add("Pagination", JsonConvert.SerializeObject(paginationHeader, cameCaseFormatter)); //... e aplica aqui
            response.Headers.Add("Access-Control-Expose-Headers", "Pagination");
        }

         public static int CalculateAge(this DateTime theDatetime){
            var age = DateTime.Today.Year - theDatetime.Year;
            if(theDatetime.AddYears(age) > DateTime.Today)
                age--;

            return age;
         }
    }

   
}