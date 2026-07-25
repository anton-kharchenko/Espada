using Espada.Db.Enums;

namespace Espada.Db.Extensions;

internal static class DatabaseCommandExtensions
{
    public static bool RequiresForce(this DatabaseCommandType command) => command == DatabaseCommandType.Reset;
}