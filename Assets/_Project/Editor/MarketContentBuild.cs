using System.IO;
using ChoDoLa.Gameplay;
using UnityEditor.Build;
using UnityEngine;

namespace ChoDoLa.Editor
{
    internal sealed class MarketContentBuild : BuildPlayerProcessor
    {
        public override void PrepareForBuild(BuildPlayerContext context)
        {
            var path = Path.GetFullPath(Path.Combine(Application.dataPath, "..", MarketRunHost.SourceContentPath));
            context.AddAdditionalPathToStreamingAssets(path, MarketRunHost.PlayerContentName);
        }
    }
}
