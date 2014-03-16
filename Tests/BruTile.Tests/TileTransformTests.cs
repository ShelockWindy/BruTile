﻿using BruTile.Predefined;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BruTile.Tests
{
    [TestFixture]
    public class TileTransformTests
    {
        private const double Epsilon = 0.000001;

        [Test]
        public void TileToWorldShouldReturnCorrectExtent()
        {
            // arrange
            var range = new TileRange(1, 2);
            var schema = new SphericalMercatorWorldSchema();
            var expectedExtent = new Extent(-15028131.257989,-10018754.173189,-10018754.173189,-5009377.088389);
              
            // act
            var extent = TileTransform.TileToWorld(range, "3", schema);

            // assert
            Assert.AreEqual(extent.MinX, expectedExtent.MinX, 0.0001);
            Assert.AreEqual(extent.MinY, expectedExtent.MinY, 0.0001);
            Assert.AreEqual(extent.MaxX, expectedExtent.MaxX, 0.0001);
            Assert.AreEqual(extent.MaxY, expectedExtent.MaxY, 0.0001);
        }

        [Test]
        public void WorldToTileShouldReturnCorrectTileRange()
        {
            // arrange
            var expectedRange = new TileRange(1, 2);
            var schema = new SphericalMercatorWorldSchema();
            var extent = new Extent(-15028130, -10018753, -10018755, -5009378);
            
            // act
            var range = TileTransform.WorldToTile(extent, "3", schema);

            // assert
            Assert.AreEqual(range, expectedRange);
        }
        [Test]
        public void GetTilesInViewWithBiggerExtentThanTileSchemaExtentReturnsCorrectNumberOfTiles()
        {
            // arrange
            var schema = new SphericalMercatorInvertedWorldSchema();
            var requestExtent = GrowExtent(schema.Extent, schema.Extent.Width);

            // act
            var tileInfos = schema.GetTilesInView(requestExtent, "0").ToList();

            // assert
            Assert.True(tileInfos.Count == 1);

        }

        private static Extent GrowExtent(Extent extent, double amount)
        {
            return new Extent(
                extent.MinX - amount,
                extent.MinY - amount,
                extent.MaxX + amount,
                extent.MaxY + amount);
        }

        [Test]
        public void TileSchemaWithExtentThatDoesOriginateInOriginShouldReturnCorrectNumberOfTiles()
        {
            // arrange
            var schemaExtent = new Extent(187009,331184,187189,331290);
            var schema = new WkstNederlandSchema {Extent = schemaExtent, OriginY = -100000};
            var requestExtent = GrowExtent(schemaExtent, schemaExtent.Width);
            
            // act
            var tileInfos = schema.GetTilesInView(requestExtent, "14").ToList();

            // assert
            Assert.True(TilesWithinEnvelope(tileInfos, schemaExtent));
            Assert.True(Math.Abs(TileAreaWithinEnvelope(tileInfos, schemaExtent) - schemaExtent.Area) < Epsilon);
        }

        [Test]
        public void TileSchemaWithExtentThatDoesOriginateInOriginAndWithInverteYShouldReturnCorrectNumberOfTiles()
        {
            // arrange
            var schemaExtent = new Extent(187009, 331184, 187189, 331290);
            var schema = new WkstNederlandSchema { Extent = schemaExtent, OriginY = -22598.080, Axis = AxisDirection.InvertedY };
            var requestExtent = GrowExtent(schemaExtent, schemaExtent.Width);

            // act
            var tileInfos = schema.GetTilesInView(requestExtent, "14");

            // assert
            Assert.True(TilesWithinEnvelope(tileInfos, schemaExtent));
            Assert.True(Math.Abs(TileAreaWithinEnvelope(tileInfos, schemaExtent) - schemaExtent.Area) < Epsilon);
        }

        private static bool TilesWithinEnvelope(IEnumerable<TileInfo> tileInfos, Extent evenlope)
        {
            return tileInfos.All(tileInfo => evenlope.Intersects(tileInfo.Extent));
        }

        private static double TileAreaWithinEnvelope(IEnumerable<TileInfo> tileInfos, Extent envelope)
        {
            return tileInfos.Sum(tileInfo => envelope.Intersect(tileInfo.Extent).Area);
        }

        [Test]
        public void TileSchemaWithExtentThatDoesNotStartInOriginShouldReturnNoTiles()
        {
            // arrange
            var schema = new WkstNederlandSchema {Extent = new Extent(187036, 331205, 187202, 331291)};
            var mapExtent = new Extent(187256.999043765,331197.712996388,187437.576002535,331303.350517269);

            // act
            var tileInfos = schema.GetTilesInView(mapExtent, "14");

            // assert
            Assert.AreEqual(tileInfos.Count(), 0);
        }
    }
}
