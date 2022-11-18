//namespace ParcelRegistry.Tests.AggregateTests
//{
//    using System;
//    using System.Collections.Generic;
//    using System.Linq;
//    using AutoFixture;
//    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
//    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
//    using Building;
//    using Building.Events;
//    using Fixtures;
//    using FluentAssertions;
//    using Legacy.Autofixture;
//    using Moq;
//    using Xunit;
//    using Xunit.Abstractions;

//    public class RestoreBuildingSnapshotTests : BuildingRegistryTest
//    {
//        private readonly Building _sut;
//        private readonly BuildingSnapshot _buildingSnapshot;

//        public RestoreBuildingSnapshotTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
//        {
//            var random = new Random(Fixture.Create<int>());

//            Fixture.Customize(new InfrastructureCustomization());
//            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
//            Fixture.Customize(new WithBuildingStatus());
//            Fixture.Customize(new WithBuildingUnitStatus());
//            Fixture.Customize(new WithBuildingUnitFunction());
//            Fixture.Customize(new WithBuildingUnitPositionGeometryMethod());
//            Fixture.Customize(new WithBuildingGeometryMethod());


//            Fixture.Register<Fixture, IEnumerable<BuildingUnit>>(fixture =>
//            {
//                fixture.Customize(new WithValidExtendedWkbPoint());
//                var buildingUnits = new List<BuildingUnit>();
//                var buildingUnitData = fixture.Build<BuildingSnapshot.BuildingUnitData>()
//                    .FromFactory(() => new BuildingSnapshot.BuildingUnitData(
//                        fixture.Create<BuildingUnitPersistentLocalId>(),
//                        fixture.Create<BuildingUnitFunction>(),
//                        fixture.Create<BuildingUnitStatus>(),
//                        fixture.Create<BuildingUnitPosition>(),
//                        fixture.Create<IEnumerable<int>>(),
//                        fixture.Create<bool>(),
//                        fixture.Create<bool>(),
//                        fixture.Create<string>(),
//                        fixture.Create<ProvenanceData>()))
//                  .CreateMany(random.Next(2, 10));

//                var streetNames = fixture.Build<IEnumerable<BuildingUnit>>().FromFactory(() =>
//                {
//                    var units = new List<BuildingUnit>();

//                    foreach (var data in buildingUnitData)
//                    {
//                        var buildingUnit = new BuildingUnit(o => { });
//                        buildingUnit.RestoreSnapshot(fixture.Create<BuildingPersistentLocalId>(), data);

//                        units.Add(buildingUnit);
//                    }

//                    return units;
//                }).Create().ToList();

//                buildingUnits.AddRange(streetNames);

//                return buildingUnits;
//            });

//            Fixture.Customize(new WithValidExtendedWkbPolygon());

//            _sut = new BuildingFactory(IntervalStrategy.Default, Mock.Of<IAddCommonBuildingUnit>()).Create();
//            _buildingSnapshot = Fixture.Create<BuildingSnapshot>();
//            _sut.Initialize(new List<object> { _buildingSnapshot });
//        }

//        [Fact]
//        public void ThenAggregateBuildingStateIsExpected()
//        {
//            _sut.BuildingPersistentLocalId.Should().Be(new BuildingPersistentLocalId(_buildingSnapshot.BuildingPersistentLocalId));

//            _sut.BuildingGeometry.Should().Be(new BuildingGeometry(
//                new ExtendedWkbGeometry(_buildingSnapshot.ExtendedWkbGeometry),
//                BuildingGeometryMethod.Parse(_buildingSnapshot.GeometryMethod)));

//            _sut.BuildingStatus.Should().Be(BuildingStatus.Parse(_buildingSnapshot.BuildingStatus));
//            _sut.IsRemoved.Should().Be(_buildingSnapshot.IsRemoved);
//            _sut.LastEventHash.Should().Be(_buildingSnapshot.LastEventHash);
//            _sut.LastProvenanceData.Should().Be(_buildingSnapshot.LastProvenanceData);
//        }

//        [Fact]
//        public void ThenAggregateBuildingUnitsStateAreExpected()
//        {
//            _sut.BuildingUnits.Should().NotBeEmpty();
//            foreach (var buildingUnit in _sut.BuildingUnits)
//            {
//                var snapshotBuildingUnit = _buildingSnapshot.BuildingUnits.SingleOrDefault(x => x.BuildingUnitPersistentLocalId == buildingUnit.BuildingUnitPersistentLocalId);

//                snapshotBuildingUnit.Should().NotBeNull();

//                buildingUnit.Function.Should().Be(BuildingUnitFunction.Parse(snapshotBuildingUnit.Function));
//                buildingUnit.Status.Should().Be(BuildingUnitStatus.Parse(snapshotBuildingUnit.Status));

//                buildingUnit.BuildingUnitPosition.Should().Be(new BuildingUnitPosition(
//                    new ExtendedWkbGeometry(snapshotBuildingUnit.ExtendedWkbGeometry),
//                    BuildingUnitPositionGeometryMethod.Parse(snapshotBuildingUnit.GeometryMethod)));

//                buildingUnit.AddressPersistentLocalIds
//                    .Should()
//                    .BeEquivalentTo(
//                    snapshotBuildingUnit.AddressPersistentLocalIds.Select(x => new AddressPersistentLocalId(x)));

//                buildingUnit.IsRemoved.Should().Be(snapshotBuildingUnit.IsRemoved);
//                buildingUnit.HasDeviation.Should().Be(snapshotBuildingUnit.HasDeviation);

//                buildingUnit.LastProvenanceData.Should().Be(snapshotBuildingUnit.LastProvenanceData);
//                buildingUnit.LastEventHash.Should().Be(snapshotBuildingUnit.LastEventHash);
//            }
//        }
//    }
//}
