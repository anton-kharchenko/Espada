using Espada.Domain.SeedWork;

namespace Espada.Api.Extensions
{
    internal static class EnumerationExtensions
    {
        public static T? ToEnumeration<T>(this int id) where T : Enumeration
        {
            return Enumeration.GetAll<T>().SingleOrDefault(value => value.Id == id);
        }
    }
}