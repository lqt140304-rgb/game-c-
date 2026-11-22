#if UNITY_EDITOR
using System.Linq;
using ChoDoLa.Town;
using ChoDoLa.Town.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace ChoDoLa.Town.Editor.Tests
{
    public class TownBlockoutBuilderTests
    {
        [Test]
        public void Build_CreatesSceneWithEightSlotMarkers()
        {
            TownBlockoutBuilder.Build();
            Assert.That(Object.FindObjectsByType<ShopSlotMarker>(FindObjectsSortMode.None).Length, Is.EqualTo(8));
        }

        [Test]
        public void Markers_HaveNoSolidCollider()
        {
            TownBlockoutBuilder.Build();
            foreach (var m in Object.FindObjectsByType<ShopSlotMarker>(FindObjectsSortMode.None))
            {
                Assert.That(m.GetComponentsInChildren<Collider>().All(c => c.isTrigger), Is.True);
            }
        }

        [Test]
        public void Build_IsIdempotent()
        {
            TownBlockoutBuilder.Build();
            TownBlockoutBuilder.Build();
            Assert.That(AssetDatabase.FindAssets("t:Prefab (1)", new[] { "Assets/_Project/Prefabs/Town" }), Is.Empty);
            Assert.That(Object.FindObjectsByType<ShopSlotMarker>(FindObjectsSortMode.None).Length, Is.EqualTo(8));
        }

        [Test]
        public void Build_PlacesPlayerAtInnPorch()
        {
            TownBlockoutBuilder.Build();
            var p = GameObject.Find("Player").transform.position;
            Assert.That(new Vector2(p.x, p.z), Is.EqualTo(new Vector2(0f, 4f)));
        }

        [Test]
        public void Ramps_TopSurfaceMeetsGroundAndDeck()
        {
            TownBlockoutBuilder.Build();
            var ramp = GameObject.Find("Ramp_W").transform;
            Vector3 topCenter = ramp.position + ramp.up * (ramp.localScale.y * 0.5f);
            Vector3 bottomEdge = topCenter - ramp.forward * (ramp.localScale.z * 0.5f);
            Vector3 topEdge = topCenter + ramp.forward * (ramp.localScale.z * 0.5f);
            Assert.That(bottomEdge.y, Is.EqualTo(0f).Within(0.01f));
            Assert.That(topEdge.y, Is.EqualTo(3.8f).Within(0.01f));
        }
    }
}
#endif
