using SoundCloudExplode.Exceptions;

namespace SoundCloudExplode.Common;

internal static class Limitor
{
    public static void Validate(int limit)
    {
        if (limit is < Constants.MinLimit or > Constants.MaxLimit)
        {
            throw new SoundcloudExplodeException(
                $"Limit must be between {Constants.MinLimit} and {Constants.MaxLimit}"
            );
        }
    }
}
