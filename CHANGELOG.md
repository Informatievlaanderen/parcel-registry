# [3.19.0](https://github.com/informatievlaanderen/parcel-registry/compare/v3.18.1...v3.19.0) (2021-01-30)


### Features

* add sync tags on events ([07b1a3c](https://github.com/informatievlaanderen/parcel-registry/commit/07b1a3c235e2c94028721aae69919cfae09641db))

## [3.18.1](https://github.com/informatievlaanderen/parcel-registry/compare/v3.18.0...v3.18.1) (2021-01-29)


### Bug Fixes

* remove sync alternate links ([2d90806](https://github.com/informatievlaanderen/parcel-registry/commit/2d90806d4414c30f7dd12c566fe8e074b17160c3))

# [3.18.0](https://github.com/informatievlaanderen/parcel-registry/compare/v3.17.8...v3.18.0) (2021-01-12)


### Features

* add syndication status to projector api GRAR-1567 ([14a13a3](https://github.com/informatievlaanderen/parcel-registry/commit/14a13a35834ecda45a358e0c624e23452cb152dc))

## [3.17.8](https://github.com/informatievlaanderen/parcel-registry/compare/v3.17.7...v3.17.8) (2021-01-07)


### Bug Fixes

* improve cache status page GRAR-1734 ([e59ff0b](https://github.com/informatievlaanderen/parcel-registry/commit/e59ff0bdad8354d6ea574599dc81d9bce8688a3f))

## [3.17.7](https://github.com/informatievlaanderen/parcel-registry/compare/v3.17.6...v3.17.7) (2021-01-07)


### Bug Fixes

* update deps ([a825831](https://github.com/informatievlaanderen/parcel-registry/commit/a825831d9023b618abfff1640ee35c636a1b9806))

## [3.17.6](https://github.com/informatievlaanderen/parcel-registry/compare/v3.17.5...v3.17.6) (2020-12-28)


### Bug Fixes

* update basisregisters api dependency ([55fbfc8](https://github.com/informatievlaanderen/parcel-registry/commit/55fbfc8df8ddbd3fd521c9841e2825b4980a2760))

## [3.17.5](https://github.com/informatievlaanderen/parcel-registry/compare/v3.17.4...v3.17.5) (2020-12-21)


### Bug Fixes

* move to 5.0.1 ([059864f](https://github.com/informatievlaanderen/parcel-registry/commit/059864f96b9f49099c13e12eb5cc16dd9c2fdb0d))

## [3.17.4](https://github.com/informatievlaanderen/parcel-registry/compare/v3.17.3...v3.17.4) (2020-12-11)


### Bug Fixes

* correct remove complete migration ([b433dc4](https://github.com/informatievlaanderen/parcel-registry/commit/b433dc4e7ede539d419e0d97e159f864628304e4))

## [3.17.3](https://github.com/informatievlaanderen/parcel-registry/compare/v3.17.2...v3.17.3) (2020-12-03)

## [3.17.2](https://github.com/informatievlaanderen/parcel-registry/compare/v3.17.1...v3.17.2) (2020-12-03)


### Bug Fixes

* parcelStatus should be null in syndication GRAR-1651 ([a623530](https://github.com/informatievlaanderen/parcel-registry/commit/a62353066bbdaf8d078f692d55c089874babf619))

## [3.17.1](https://github.com/informatievlaanderen/parcel-registry/compare/v3.17.0...v3.17.1) (2020-11-18)


### Bug Fixes

* remove set-env usage in gh-actions ([e75378b](https://github.com/informatievlaanderen/parcel-registry/commit/e75378b78e8b224610ba9a60e35fb66d746bf796))

# [3.17.0](https://github.com/informatievlaanderen/parcel-registry/compare/v3.16.4...v3.17.0) (2020-11-16)


### Bug Fixes

* fixed tests ioc ([e7286c1](https://github.com/informatievlaanderen/parcel-registry/commit/e7286c1411f42ba1a611dfb35774dd3d6c9d99dc))
* when ParcelWasRemoved also remove linked addresses ([c1d872b](https://github.com/informatievlaanderen/parcel-registry/commit/c1d872bb20a49e227b1e4d2763d755e02a9e257e))


### Features

* add fix command for GRAR-1637 ([4c458fb](https://github.com/informatievlaanderen/parcel-registry/commit/4c458fb31f92aaf37aadcb0cd137c2052e7c0673))
* add ParcelWasRecovered event ([f80c23c](https://github.com/informatievlaanderen/parcel-registry/commit/f80c23ce0ca8aee308afc15dc7db4a621c93ba89))
* update extract and lastchanged projections for Recovered event ([f34b802](https://github.com/informatievlaanderen/parcel-registry/commit/f34b802ddb91cd7d9d59f5985e88592daca669ee))
* update legacy projections for Recovered event ([f9cdd6b](https://github.com/informatievlaanderen/parcel-registry/commit/f9cdd6b83535adb0b4501207a7e0406b289e9d09))

## [3.16.4](https://github.com/informatievlaanderen/parcel-registry/compare/v3.16.3...v3.16.4) (2020-11-13)


### Bug Fixes

* display sync response example as correct xml GRAR-1599 ([a156ac9](https://github.com/informatievlaanderen/parcel-registry/commit/a156ac90d5922def2d4086a864a9bec061d81c16))
* upgrade swagger GRAR-1599 ([375057f](https://github.com/informatievlaanderen/parcel-registry/commit/375057fb09eea60f23e0fbdf153dd279b8067b92))
* use production url for sync examples ([00c3d60](https://github.com/informatievlaanderen/parcel-registry/commit/00c3d600799bd1b0f38d23f9a79a03e950e3a58f))

## [3.16.3](https://github.com/informatievlaanderen/parcel-registry/compare/v3.16.2...v3.16.3) (2020-11-12)


### Bug Fixes

* use event name instead of type for sync xml serialization ([5cbcfa5](https://github.com/informatievlaanderen/parcel-registry/commit/5cbcfa562360eecaf2b055c06f3972cc13905908))

## [3.16.2](https://github.com/informatievlaanderen/parcel-registry/compare/v3.16.1...v3.16.2) (2020-11-09)


### Bug Fixes

* remove Take(5) from crab queries ([f51facb](https://github.com/informatievlaanderen/parcel-registry/commit/f51facb1ebe6d1489f9af1a0aae92b1aed339643))

## [3.16.1](https://github.com/informatievlaanderen/parcel-registry/compare/v3.16.0...v3.16.1) (2020-11-06)


### Bug Fixes

* logging ([d5bb514](https://github.com/informatievlaanderen/parcel-registry/commit/d5bb5144a9ef8dc72c48f0da6a7cd379827143e6))
* logging ([e45ed9d](https://github.com/informatievlaanderen/parcel-registry/commit/e45ed9d12cf76a538fb280d8cb8fba712fb7d723))
* logging ([26eb879](https://github.com/informatievlaanderen/parcel-registry/commit/26eb87977d7d9c2518335baf4e759bdbbf5fb02b))
* logging ([8519364](https://github.com/informatievlaanderen/parcel-registry/commit/85193647520c63909d5425536590b16546727664))
* logging ([efac05d](https://github.com/informatievlaanderen/parcel-registry/commit/efac05d32bf7e5e63a54af4bf4132770e3119864))
* logging ([a83b0d3](https://github.com/informatievlaanderen/parcel-registry/commit/a83b0d3e546b3df84c983323f09a4757541c0176))

# [3.16.0](https://github.com/informatievlaanderen/parcel-registry/compare/v3.15.0...v3.16.0) (2020-10-27)


### Features

* add error message for syndication projections ([819feea](https://github.com/informatievlaanderen/parcel-registry/commit/819feea7e02362466ed210f66f107a69a8d57d59))

# [3.15.0](https://github.com/informatievlaanderen/parcel-registry/compare/v3.14.0...v3.15.0) (2020-10-27)


### Features

* update projector with gap detection and extended status api ([06d6d60](https://github.com/informatievlaanderen/parcel-registry/commit/06d6d60aea232baa595ad47110b0b27c114f6c09))

# [3.14.0](https://github.com/informatievlaanderen/parcel-registry/compare/v3.13.0...v3.14.0) (2020-10-15)


### Features

* add cache status to projector api ([5862db1](https://github.com/informatievlaanderen/parcel-registry/commit/5862db1a227cc599646d6d12e1f6956e5cd35b4f))

# [3.13.0](https://github.com/informatievlaanderen/parcel-registry/compare/v3.12.2...v3.13.0) (2020-10-14)


### Features

* add status to api legacy list ([4aa53f3](https://github.com/informatievlaanderen/parcel-registry/commit/4aa53f3bd946455141f58af15bd4c218968dc5d8))

## [3.12.2](https://github.com/informatievlaanderen/parcel-registry/compare/v3.12.1...v3.12.2) (2020-10-13)


### Bug Fixes

* correct category GRAR-1450 ([96822ff](https://github.com/informatievlaanderen/parcel-registry/commit/96822ffe8e470ee0625643f24ac50ec99e1706ba))
* correct metadata GRAR-1446 GRAR-1450 GRAR-1444 GRAR-1422 ([24aed69](https://github.com/informatievlaanderen/parcel-registry/commit/24aed697f8715b99e23cfddb8cee0bd58cddb84c))
* correct next link sync feed GRAR-1422 ([bff9fc5](https://github.com/informatievlaanderen/parcel-registry/commit/bff9fc588864032c0c8726c32f6908dab862b179))

## [3.12.1](https://github.com/informatievlaanderen/parcel-registry/compare/v3.12.0...v3.12.1) (2020-10-05)


### Bug Fixes

* run projection using the feedprojector GRAR-1562 ([3eedd79](https://github.com/informatievlaanderen/parcel-registry/commit/3eedd79bbd175a206c4589d35db7739a8feeb5a7))

# [3.12.0](https://github.com/informatievlaanderen/parcel-registry/compare/v3.11.8...v3.12.0) (2020-09-22)


### Features

* add import status endpoint GRAR-1400 ([e9d71ab](https://github.com/informatievlaanderen/parcel-registry/commit/e9d71ab9dc6486f3337f67adf619ce50da63e345))

## [3.11.8](https://github.com/informatievlaanderen/parcel-registry/compare/v3.11.7...v3.11.8) (2020-09-22)


### Bug Fixes

* move to 3.1.8 ([851fe6c](https://github.com/informatievlaanderen/parcel-registry/commit/851fe6c826c489e98eab5f649d0c48c865dbb486))

## [3.11.7](https://github.com/informatievlaanderen/parcel-registry/compare/v3.11.6...v3.11.7) (2020-09-11)


### Bug Fixes

* remove Modification from xml GRAR-1529 ([50949f9](https://github.com/informatievlaanderen/parcel-registry/commit/50949f9ecd34703184bb102bf74ce2fd08fee149))

## [3.11.6](https://github.com/informatievlaanderen/parcel-registry/compare/v3.11.5...v3.11.6) (2020-09-11)


### Bug Fixes

* update packages to fix null operator/reason GRAR-1535 ([aadc093](https://github.com/informatievlaanderen/parcel-registry/commit/aadc093fdf8a11fdb1a5e0be6356a8a76c898528))

## [3.11.5](https://github.com/informatievlaanderen/parcel-registry/compare/v3.11.4...v3.11.5) (2020-09-11)


### Bug Fixes

* remove paging response header in sync ([9fdc6fa](https://github.com/informatievlaanderen/parcel-registry/commit/9fdc6fa0153654d4b4f2e428473a7a27eea121cb))

## [3.11.4](https://github.com/informatievlaanderen/parcel-registry/compare/v3.11.3...v3.11.4) (2020-09-10)


### Bug Fixes

* add sync with correct timestamp configuration GRAR-1483 ([4d641c6](https://github.com/informatievlaanderen/parcel-registry/commit/4d641c672116a7c8903d0bc81a335bbb7ba0fc36))

## [3.11.3](https://github.com/informatievlaanderen/parcel-registry/compare/v3.11.2...v3.11.3) (2020-09-03)


### Bug Fixes

* null organisation defaults to unknown ([53d6d8b](https://github.com/informatievlaanderen/parcel-registry/commit/53d6d8bb8096f00aef17363b9f540037323c7ede))

## [3.11.2](https://github.com/informatievlaanderen/parcel-registry/compare/v3.11.1...v3.11.2) (2020-09-02)


### Bug Fixes

* upgrade common to fix sync author ([9ba749c](https://github.com/informatievlaanderen/parcel-registry/commit/9ba749c0e34ccac7a881404e9cad5756a9df75dd))

## [3.11.1](https://github.com/informatievlaanderen/parcel-registry/compare/v3.11.0...v3.11.1) (2020-07-19)


### Bug Fixes

* move to 3.1.6 ([e7dc0ea](https://github.com/informatievlaanderen/parcel-registry/commit/e7dc0eab6df85ca2fc639b6b7670469c47e57491))

# [3.11.0](https://github.com/informatievlaanderen/parcel-registry/compare/v3.10.19...v3.11.0) (2020-07-14)


### Features

* add timestamp to sync provenance GRAR-1451 ([f79c6be](https://github.com/informatievlaanderen/parcel-registry/commit/f79c6bebb5bbde3f1a475eb374622738459013fd))

## [3.10.19](https://github.com/informatievlaanderen/parcel-registry/compare/v3.10.18...v3.10.19) (2020-07-13)


### Bug Fixes

* update dependencies ([8fa1053](https://github.com/informatievlaanderen/parcel-registry/commit/8fa1053b8c51fb57793f612d08d87c506596bc4c))
* use typed embed value GRAR-1465 ([b3d164a](https://github.com/informatievlaanderen/parcel-registry/commit/b3d164a1cb4d448db419849a8aff3e92079ecd63))

## [3.10.18](https://github.com/informatievlaanderen/parcel-registry/compare/v3.10.17...v3.10.18) (2020-07-10)


### Bug Fixes

* correct author, entry links atom feed + example GRAR-1443 GRAR-1447 ([d65fbcc](https://github.com/informatievlaanderen/parcel-registry/commit/d65fbcc27d085cc4a278390abdc9975165581596))

## [3.10.17](https://github.com/informatievlaanderen/parcel-registry/compare/v3.10.16...v3.10.17) (2020-07-10)


### Bug Fixes

* enums were not correctly serialized in syndication event GRAR-1490 ([5fbdd91](https://github.com/informatievlaanderen/parcel-registry/commit/5fbdd919ec52ed0b2fe50b9bd9f7c030f20d01f5))

## [3.10.16](https://github.com/informatievlaanderen/parcel-registry/compare/v3.10.15...v3.10.16) (2020-07-07)


### Bug Fixes

* add processed file + domain fixes ([d4f4067](https://github.com/informatievlaanderen/parcel-registry/commit/d4f4067c224362da174b83d7ee21bc3a286747f9))

## [3.10.15](https://github.com/informatievlaanderen/parcel-registry/compare/v3.10.14...v3.10.15) (2020-07-07)


### Bug Fixes

* add command to fix GRAR-1475 and add test project to find affected ([2a11821](https://github.com/informatievlaanderen/parcel-registry/commit/2a11821f7b57c6559163d57a1e1d0414855a6700))
* add command to fix GRAR-1475 and add test project to find affected ([b1264a7](https://github.com/informatievlaanderen/parcel-registry/commit/b1264a7b7ffc7976cc94fe021ef1427449a643cd))

## [3.10.14](https://github.com/informatievlaanderen/parcel-registry/compare/v3.10.13...v3.10.14) (2020-07-06)


### Bug Fixes

* after removed / retired the correct events can be applied ([e4c2a5b](https://github.com/informatievlaanderen/parcel-registry/commit/e4c2a5b6303f81922e942636ccfa761d02e32ed6))

## [3.10.13](https://github.com/informatievlaanderen/parcel-registry/compare/v3.10.12...v3.10.13) (2020-06-23)


### Bug Fixes

* configure baseurls for all problemdetails GRAR-1357 ([44aa6ef](https://github.com/informatievlaanderen/parcel-registry/commit/44aa6efb2d0308a14231fbeeb3f1a1135567ed59))

## [3.10.12](https://github.com/informatievlaanderen/parcel-registry/compare/v3.10.11...v3.10.12) (2020-06-22)


### Bug Fixes

* configure baseurls for all problemdetails GRAR-1358 GRAR-1357 ([1d14640](https://github.com/informatievlaanderen/parcel-registry/commit/1d14640feb0a52cd6178efc76262959a2765ad58))

## [3.10.11](https://github.com/informatievlaanderen/parcel-registry/compare/v3.10.10...v3.10.11) (2020-06-19)


### Bug Fixes

* move to 3.1.5 ([6bcb0eb](https://github.com/informatievlaanderen/parcel-registry/commit/6bcb0ebbc4c9a3ba5c6637c70c93d4337be3c2f1))

## [3.10.10](https://github.com/informatievlaanderen/parcel-registry/compare/v3.10.9...v3.10.10) (2020-06-08)


### Bug Fixes

* build msil version for public api ([9adb8be](https://github.com/informatievlaanderen/parcel-registry/commit/9adb8be9451e6d541c589595091f1896a159b458))

## [3.10.9](https://github.com/informatievlaanderen/parcel-registry/compare/v3.10.8...v3.10.9) (2020-05-29)


### Bug Fixes

* update dependencies GRAR-752 ([7ef70da](https://github.com/informatievlaanderen/parcel-registry/commit/7ef70daf67166126000b4fdb9172e05650d7f5b3))

## [3.10.8](https://github.com/informatievlaanderen/parcel-registry/compare/v3.10.7...v3.10.8) (2020-05-20)


### Bug Fixes

* force build ([7518f2b](https://github.com/informatievlaanderen/parcel-registry/commit/7518f2b5f1b6bafde5b110e978c340794de100e9))

## [3.10.7](https://github.com/informatievlaanderen/parcel-registry/compare/v3.10.6...v3.10.7) (2020-05-20)


### Bug Fixes

* add build badge ([97c630a](https://github.com/informatievlaanderen/parcel-registry/commit/97c630afab76b6db73bd4ac1940be83a81d87997))

## [3.10.6](https://github.com/informatievlaanderen/parcel-registry/compare/v3.10.5...v3.10.6) (2020-05-19)


### Bug Fixes

* move to 3.1.4 and gh actions ([6428c6a](https://github.com/informatievlaanderen/parcel-registry/commit/6428c6a2d44dac2161a806cb4c84e76508221aa8))

## [3.10.5](https://github.com/informatievlaanderen/parcel-registry/compare/v3.10.4...v3.10.5) (2020-04-28)


### Bug Fixes

* update grar dependencies GRAR-412 ([b2e56a5](https://github.com/informatievlaanderen/parcel-registry/commit/b2e56a5))

## [3.10.4](https://github.com/informatievlaanderen/parcel-registry/compare/v3.10.3...v3.10.4) (2020-04-14)


### Bug Fixes

* update packages ([cf78195](https://github.com/informatievlaanderen/parcel-registry/commit/cf78195))

## [3.10.3](https://github.com/informatievlaanderen/parcel-registry/compare/v3.10.2...v3.10.3) (2020-04-10)


### Bug Fixes

* update grar common packages ([342d402](https://github.com/informatievlaanderen/parcel-registry/commit/342d402))

## [3.10.2](https://github.com/informatievlaanderen/parcel-registry/compare/v3.10.1...v3.10.2) (2020-04-10)


### Bug Fixes

* update packages for import batch timestamps ([9af810a](https://github.com/informatievlaanderen/parcel-registry/commit/9af810a))

## [3.10.1](https://github.com/informatievlaanderen/parcel-registry/compare/v3.10.0...v3.10.1) (2020-04-06)


### Bug Fixes

* set name for importer feedname ([b999b04](https://github.com/informatievlaanderen/parcel-registry/commit/b999b04))

# [3.10.0](https://github.com/informatievlaanderen/parcel-registry/compare/v3.9.1...v3.10.0) (2020-04-03)


### Features

* upgrade projection handling to include errmessage lastchangedlist ([38df13b](https://github.com/informatievlaanderen/parcel-registry/commit/38df13b))

## [3.9.1](https://github.com/informatievlaanderen/parcel-registry/compare/v3.9.0...v3.9.1) (2020-03-27)


### Bug Fixes

* set sync dates to belgian timezone ([6f16422](https://github.com/informatievlaanderen/parcel-registry/commit/6f16422))

# [3.9.0](https://github.com/informatievlaanderen/parcel-registry/compare/v3.8.1...v3.9.0) (2020-03-26)


### Features

* add status filter on list ([8e648d2](https://github.com/informatievlaanderen/parcel-registry/commit/8e648d2))

## [3.8.1](https://github.com/informatievlaanderen/parcel-registry/compare/v3.8.0...v3.8.1) (2020-03-23)


### Bug Fixes

* correct versie id type to string fix ([8e42918](https://github.com/informatievlaanderen/parcel-registry/commit/8e42918))
* update grar common to fix versie id type ([cbf04f6](https://github.com/informatievlaanderen/parcel-registry/commit/cbf04f6))

# [3.8.0](https://github.com/informatievlaanderen/parcel-registry/compare/v3.7.3...v3.8.0) (2020-03-20)


### Features

* send mail when importer crashes ([bd93584](https://github.com/informatievlaanderen/parcel-registry/commit/bd93584))

## [3.7.3](https://github.com/informatievlaanderen/parcel-registry/compare/v3.7.2...v3.7.3) (2020-03-18)


### Bug Fixes

* make status mapping more obvious ([94a278c](https://github.com/informatievlaanderen/parcel-registry/commit/94a278c))

## [3.7.2](https://github.com/informatievlaanderen/parcel-registry/compare/v3.7.1...v3.7.2) (2020-03-18)


### Bug Fixes

* change build to new user ([d0b79c5](https://github.com/informatievlaanderen/parcel-registry/commit/d0b79c5))

## [3.7.1](https://github.com/informatievlaanderen/parcel-registry/compare/v3.7.0...v3.7.1) (2020-03-18)


### Bug Fixes

* parcel detail projection uses correct conversion for status ([be5d523](https://github.com/informatievlaanderen/parcel-registry/commit/be5d523))

# [3.7.0](https://github.com/informatievlaanderen/parcel-registry/compare/v3.6.2...v3.7.0) (2020-03-18)


### Features

* upgrade importer to netcore3 ([f8f2cf8](https://github.com/informatievlaanderen/parcel-registry/commit/f8f2cf8))

## [3.6.2](https://github.com/informatievlaanderen/parcel-registry/compare/v3.6.1...v3.6.2) (2020-03-11)


### Bug Fixes

* count parcels now count correctly ([48d92d2](https://github.com/informatievlaanderen/parcel-registry/commit/48d92d2))

## [3.6.1](https://github.com/informatievlaanderen/parcel-registry/compare/v3.6.0...v3.6.1) (2020-03-10)


### Bug Fixes

* remove json ld from populator ([40f54d9](https://github.com/informatievlaanderen/parcel-registry/commit/40f54d9))

# [3.6.0](https://github.com/informatievlaanderen/parcel-registry/compare/v3.5.11...v3.6.0) (2020-03-10)


### Features

* add totaal aantal endpoint ([235fd3e](https://github.com/informatievlaanderen/parcel-registry/commit/235fd3e))

## [3.5.11](https://github.com/informatievlaanderen/parcel-registry/compare/v3.5.10...v3.5.11) (2020-03-05)


### Bug Fixes

* set correct timestamp for sync projection ([bdb180b](https://github.com/informatievlaanderen/parcel-registry/commit/bdb180b))

## [3.5.10](https://github.com/informatievlaanderen/parcel-registry/compare/v3.5.9...v3.5.10) (2020-03-05)


### Bug Fixes

* update grar common to fix provenance ([25bf9ea](https://github.com/informatievlaanderen/parcel-registry/commit/25bf9ea))

## [3.5.9](https://github.com/informatievlaanderen/parcel-registry/compare/v3.5.8...v3.5.9) (2020-03-04)


### Bug Fixes

* bump netcore dockerfiles ([5158149](https://github.com/informatievlaanderen/parcel-registry/commit/5158149))

## [3.5.8](https://github.com/informatievlaanderen/parcel-registry/compare/v3.5.7...v3.5.8) (2020-03-03)


### Bug Fixes

* bump netcore to 3.1.2 ([e882575](https://github.com/informatievlaanderen/parcel-registry/commit/e882575))
* update dockerid detection ([99e9313](https://github.com/informatievlaanderen/parcel-registry/commit/99e9313))

## [3.5.7](https://github.com/informatievlaanderen/parcel-registry/compare/v3.5.6...v3.5.7) (2020-02-27)


### Bug Fixes

* update json serialization dependencies ([d321eb1](https://github.com/informatievlaanderen/parcel-registry/commit/d321eb1))

## [3.5.6](https://github.com/informatievlaanderen/parcel-registry/compare/v3.5.5...v3.5.6) (2020-02-24)


### Bug Fixes

* update projection handling & sync migrator ([24c7e55](https://github.com/informatievlaanderen/parcel-registry/commit/24c7e55))

## [3.5.5](https://github.com/informatievlaanderen/parcel-registry/compare/v3.5.4...v3.5.5) (2020-02-21)


### Performance Improvements

* increase performance by removing count from list ([3280182](https://github.com/informatievlaanderen/parcel-registry/commit/3280182))

## [3.5.4](https://github.com/informatievlaanderen/parcel-registry/compare/v3.5.3...v3.5.4) (2020-02-20)


### Bug Fixes

* update grar common ([a940af1](https://github.com/informatievlaanderen/parcel-registry/commit/a940af1))

## [3.5.3](https://github.com/informatievlaanderen/parcel-registry/compare/v3.5.2...v3.5.3) (2020-02-19)


### Bug Fixes

* set order in api's + add clustered index address sync ([03e2009](https://github.com/informatievlaanderen/parcel-registry/commit/03e2009))

## [3.5.2](https://github.com/informatievlaanderen/parcel-registry/compare/v3.5.1...v3.5.2) (2020-02-17)


### Bug Fixes

* upgrade packages to fix json order ([307101a](https://github.com/informatievlaanderen/parcel-registry/commit/307101a))

## [3.5.1](https://github.com/informatievlaanderen/parcel-registry/compare/v3.5.0...v3.5.1) (2020-02-14)


### Bug Fixes

* add index to list ([8f13206](https://github.com/informatievlaanderen/parcel-registry/commit/8f13206))

# [3.5.0](https://github.com/informatievlaanderen/parcel-registry/compare/v3.4.5...v3.5.0) (2020-02-12)


### Bug Fixes

* add libs to be able to build Importer ([42f64c0](https://github.com/informatievlaanderen/parcel-registry/commit/42f64c0))


### Features

* add update to importer ([aa62790](https://github.com/informatievlaanderen/parcel-registry/commit/aa62790))

## [3.4.5](https://github.com/informatievlaanderen/parcel-registry/compare/v3.4.4...v3.4.5) (2020-02-10)


### Bug Fixes

* JSON default value for nullable field ([c63f031](https://github.com/informatievlaanderen/parcel-registry/commit/c63f031))

## [3.4.4](https://github.com/informatievlaanderen/parcel-registry/compare/v3.4.3...v3.4.4) (2020-02-04)


### Bug Fixes

* instanceuri for error examples now show correctly ([650f487](https://github.com/informatievlaanderen/parcel-registry/commit/650f487))

## [3.4.3](https://github.com/informatievlaanderen/parcel-registry/compare/v3.4.2...v3.4.3) (2020-02-03)


### Bug Fixes

* correct gemeenten to percelen ([eede076](https://github.com/informatievlaanderen/parcel-registry/commit/eede076))

## [3.4.2](https://github.com/informatievlaanderen/parcel-registry/compare/v3.4.1...v3.4.2) (2020-02-03)


### Bug Fixes

* add type to problemdetails ([748069a](https://github.com/informatievlaanderen/parcel-registry/commit/748069a))

## [3.4.1](https://github.com/informatievlaanderen/parcel-registry/compare/v3.4.0...v3.4.1) (2020-02-03)


### Bug Fixes

* specify non nullable responses ([bc4945c](https://github.com/informatievlaanderen/parcel-registry/commit/bc4945c))

# [3.4.0](https://github.com/informatievlaanderen/parcel-registry/compare/v3.3.3...v3.4.0) (2020-02-01)


### Features

* upgrade netcoreapp31 and dependencies ([04307b1](https://github.com/informatievlaanderen/parcel-registry/commit/04307b1))

## [3.3.3](https://github.com/informatievlaanderen/parcel-registry/compare/v3.3.2...v3.3.3) (2020-01-28)


### Bug Fixes

* ef issues corrected ([0baa9a8](https://github.com/informatievlaanderen/parcel-registry/commit/0baa9a8))

## [3.3.2](https://github.com/informatievlaanderen/parcel-registry/compare/v3.3.1...v3.3.2) (2020-01-24)


### Bug Fixes

* add syndication to api references ([63867cc](https://github.com/informatievlaanderen/parcel-registry/commit/63867cc))

## [3.3.1](https://github.com/informatievlaanderen/parcel-registry/compare/v3.3.0...v3.3.1) (2020-01-23)


### Bug Fixes

* syndication distributedlock now runs async ([143fe5c](https://github.com/informatievlaanderen/parcel-registry/commit/143fe5c))

# [3.3.0](https://github.com/informatievlaanderen/parcel-registry/compare/v3.2.0...v3.3.0) (2020-01-23)


### Features

* upgrade projectionhandling package ([d428b64](https://github.com/informatievlaanderen/parcel-registry/commit/d428b64))

# [3.2.0](https://github.com/informatievlaanderen/parcel-registry/compare/v3.1.1...v3.2.0) (2020-01-23)


### Features

* use distributed lock for syndication ([6911862](https://github.com/informatievlaanderen/parcel-registry/commit/6911862))

## [3.1.1](https://github.com/informatievlaanderen/parcel-registry/compare/v3.1.0...v3.1.1) (2020-01-16)


### Bug Fixes

* get api's working again ([7128931](https://github.com/informatievlaanderen/parcel-registry/commit/7128931))

# [3.1.0](https://github.com/informatievlaanderen/parcel-registry/compare/v3.0.0...v3.1.0) (2020-01-03)


### Features

* allow only one projector instance ([6d6de25](https://github.com/informatievlaanderen/parcel-registry/commit/6d6de25))

# [3.0.0](https://github.com/informatievlaanderen/parcel-registry/compare/v2.3.3...v3.0.0) (2019-12-25)


### Code Refactoring

* upgrade to netcoreapp31 ([b93ce83](https://github.com/informatievlaanderen/parcel-registry/commit/b93ce83))


### BREAKING CHANGES

* Upgrade to .NET Core 3.1

## [2.3.3](https://github.com/informatievlaanderen/parcel-registry/compare/v2.3.2...v2.3.3) (2019-12-12)


### Bug Fixes

* correct extract realized status ([fec400b](https://github.com/informatievlaanderen/parcel-registry/commit/fec400b))

## [2.3.2](https://github.com/informatievlaanderen/parcel-registry/compare/v2.3.1...v2.3.2) (2019-12-11)


### Bug Fixes

* correct extract filename ([e74874b](https://github.com/informatievlaanderen/parcel-registry/commit/e74874b))

## [2.3.1](https://github.com/informatievlaanderen/parcel-registry/compare/v2.3.0...v2.3.1) (2019-12-04)


### Bug Fixes

* make the where of the view identical to the ParcelListQuery filter ([be8ea31](https://github.com/informatievlaanderen/parcel-registry/commit/be8ea31))

# [2.3.0](https://github.com/informatievlaanderen/parcel-registry/compare/v2.2.6...v2.3.0) (2019-12-03)


### Bug Fixes

* remove count from sync GR-852 ([679e4e7](https://github.com/informatievlaanderen/parcel-registry/commit/679e4e7))
* remove events connectionstring from extract ([0467765](https://github.com/informatievlaanderen/parcel-registry/commit/0467765))


### Features

* add view to count parcels GR-852 ([9ecc4ae](https://github.com/informatievlaanderen/parcel-registry/commit/9ecc4ae))

## [2.2.6](https://github.com/informatievlaanderen/parcel-registry/compare/v2.2.5...v2.2.6) (2019-11-29)

## [2.2.5](https://github.com/informatievlaanderen/parcel-registry/compare/v2.2.4...v2.2.5) (2019-11-21)


### Bug Fixes

* add a sorting to legacy api detail addresses ([6bf1227](https://github.com/informatievlaanderen/parcel-registry/commit/6bf1227))

## [2.2.4](https://github.com/informatievlaanderen/parcel-registry/compare/v2.2.3...v2.2.4) (2019-10-24)


### Bug Fixes

* push to correct repo ([15a059a](https://github.com/informatievlaanderen/parcel-registry/commit/15a059a))
* upgrade grar common ([1638edd](https://github.com/informatievlaanderen/parcel-registry/commit/1638edd))

## [2.2.3](https://github.com/informatievlaanderen/parcel-registry/compare/v2.2.2...v2.2.3) (2019-09-30)


### Bug Fixes

* check removed before completeness GR-900 ([1172158](https://github.com/informatievlaanderen/parcel-registry/commit/1172158))

## [2.2.2](https://github.com/informatievlaanderen/parcel-registry/compare/v2.2.1...v2.2.2) (2019-09-26)


### Bug Fixes

* update asset to fix importer ([cd266f0](https://github.com/informatievlaanderen/parcel-registry/commit/cd266f0))

## [2.2.1](https://github.com/informatievlaanderen/parcel-registry/compare/v2.2.0...v2.2.1) (2019-09-26)


### Bug Fixes

* resume projections on startup ([e06412a](https://github.com/informatievlaanderen/parcel-registry/commit/e06412a))

# [2.2.0](https://github.com/informatievlaanderen/parcel-registry/compare/v2.1.1...v2.2.0) (2019-09-25)


### Features

* upgrade projector and removed explicit start of projections ([fb16d8a](https://github.com/informatievlaanderen/parcel-registry/commit/fb16d8a))

## [2.1.1](https://github.com/informatievlaanderen/parcel-registry/compare/v2.1.0...v2.1.1) (2019-09-19)


### Bug Fixes

* upgrade NTS ([269edb8](https://github.com/informatievlaanderen/parcel-registry/commit/269edb8))

# [2.1.0](https://github.com/informatievlaanderen/parcel-registry/compare/v2.0.0...v2.1.0) (2019-09-19)


### Features

* upgrade NTS & Shaperon packages ([29907fa](https://github.com/informatievlaanderen/parcel-registry/commit/29907fa))

# [2.0.0](https://github.com/informatievlaanderen/parcel-registry/compare/v1.14.5...v2.0.0) (2019-09-18)


### Bug Fixes

* correct relations with addresses GR-874 ([dbeb5f7](https://github.com/informatievlaanderen/parcel-registry/commit/dbeb5f7))


### BREAKING CHANGES

* New address-relation calculations

## [1.14.5](https://github.com/informatievlaanderen/parcel-registry/compare/v1.14.4...v1.14.5) (2019-09-18)


### Bug Fixes

* removed parcels are no longer shown in list GR-880 ([ba1272b](https://github.com/informatievlaanderen/parcel-registry/commit/ba1272b))

## [1.14.4](https://github.com/informatievlaanderen/parcel-registry/compare/v1.14.3...v1.14.4) (2019-09-17)


### Bug Fixes

* upgrade api for error headers ([5d4af0c](https://github.com/informatievlaanderen/parcel-registry/commit/5d4af0c))

## [1.14.3](https://github.com/informatievlaanderen/parcel-registry/compare/v1.14.2...v1.14.3) (2019-09-17)


### Bug Fixes

* use generic dbtraceconnection in syndication ([184e223](https://github.com/informatievlaanderen/parcel-registry/commit/184e223))

## [1.14.2](https://github.com/informatievlaanderen/parcel-registry/compare/v1.14.1...v1.14.2) (2019-09-17)


### Bug Fixes

* use generic dbtraceconnection ([4ddae40](https://github.com/informatievlaanderen/parcel-registry/commit/4ddae40))

## [1.14.1](https://github.com/informatievlaanderen/parcel-registry/compare/v1.14.0...v1.14.1) (2019-09-13)


### Bug Fixes

* update redis lastchangedlist to log time of lasterror ([5b93b2e](https://github.com/informatievlaanderen/parcel-registry/commit/5b93b2e))

# [1.14.0](https://github.com/informatievlaanderen/parcel-registry/compare/v1.13.12...v1.14.0) (2019-09-12)


### Features

* keep track of how many times lastchanged has errored ([1318c53](https://github.com/informatievlaanderen/parcel-registry/commit/1318c53))

## [1.13.12](https://github.com/informatievlaanderen/parcel-registry/compare/v1.13.11...v1.13.12) (2019-09-05)


### Bug Fixes

* initial jira version ([f1e2d48](https://github.com/informatievlaanderen/parcel-registry/commit/f1e2d48))

## [1.13.11](https://github.com/informatievlaanderen/parcel-registry/compare/v1.13.10...v1.13.11) (2019-09-04)


### Bug Fixes

* report correct version number ([e1a63f8](https://github.com/informatievlaanderen/parcel-registry/commit/e1a63f8))

## [1.13.10](https://github.com/informatievlaanderen/parcel-registry/compare/v1.13.9...v1.13.10) (2019-09-03)


### Bug Fixes

* update problemdetails for xml response GR-829 ([c4fa939](https://github.com/informatievlaanderen/parcel-registry/commit/c4fa939))

## [1.13.9](https://github.com/informatievlaanderen/parcel-registry/compare/v1.13.8...v1.13.9) (2019-09-02)


### Bug Fixes

* do not log to console writeline ([200770c](https://github.com/informatievlaanderen/parcel-registry/commit/200770c))
* do not log to console writeline ([d2edbad](https://github.com/informatievlaanderen/parcel-registry/commit/d2edbad))

## [1.13.8](https://github.com/informatievlaanderen/parcel-registry/compare/v1.13.7...v1.13.8) (2019-09-02)


### Bug Fixes

* properly report errors ([ce3502e](https://github.com/informatievlaanderen/parcel-registry/commit/ce3502e))

## [1.13.7](https://github.com/informatievlaanderen/parcel-registry/compare/v1.13.6...v1.13.7) (2019-08-28)


### Bug Fixes

* use longer timeout for migrations ([cce233e](https://github.com/informatievlaanderen/parcel-registry/commit/cce233e))

## [1.13.6](https://github.com/informatievlaanderen/parcel-registry/compare/v1.13.5...v1.13.6) (2019-08-28)


### Bug Fixes

* use columnstore for legacy syndication ([c4eb24f](https://github.com/informatievlaanderen/parcel-registry/commit/c4eb24f))

## [1.13.5](https://github.com/informatievlaanderen/parcel-registry/compare/v1.13.4...v1.13.5) (2019-08-28)


### Bug Fixes

* use columnstore for legacy syndication ([591762c](https://github.com/informatievlaanderen/parcel-registry/commit/591762c))

## [1.13.4](https://github.com/informatievlaanderen/parcel-registry/compare/v1.13.3...v1.13.4) (2019-08-27)


### Bug Fixes

* make datadog tracing check more for nulls ([4fb5150](https://github.com/informatievlaanderen/parcel-registry/commit/4fb5150))

## [1.13.3](https://github.com/informatievlaanderen/parcel-registry/compare/v1.13.2...v1.13.3) (2019-08-27)


### Bug Fixes

* use new desiredstate columns for projections ([ae334a5](https://github.com/informatievlaanderen/parcel-registry/commit/ae334a5))

## [1.13.2](https://github.com/informatievlaanderen/parcel-registry/compare/v1.13.1...v1.13.2) (2019-08-26)


### Bug Fixes

* use fixed datadog tracing ([9849f4e](https://github.com/informatievlaanderen/parcel-registry/commit/9849f4e))

## [1.13.1](https://github.com/informatievlaanderen/parcel-registry/compare/v1.13.0...v1.13.1) (2019-08-26)


### Bug Fixes

* fix swagger ([b7160bc](https://github.com/informatievlaanderen/parcel-registry/commit/b7160bc))

# [1.13.0](https://github.com/informatievlaanderen/parcel-registry/compare/v1.12.0...v1.13.0) (2019-08-26)


### Features

* bump to .net 2.2.6 ([03382eb](https://github.com/informatievlaanderen/parcel-registry/commit/03382eb))

# [1.12.0](https://github.com/informatievlaanderen/parcel-registry/compare/v1.11.1...v1.12.0) (2019-08-22)


### Features

* extract datavlaanderen namespace to settings fixes [#1](https://github.com/informatievlaanderen/parcel-registry/issues/1) ([f335bbb](https://github.com/informatievlaanderen/parcel-registry/commit/f335bbb))

## [1.11.1](https://github.com/informatievlaanderen/parcel-registry/compare/v1.11.0...v1.11.1) (2019-08-20)


### Bug Fixes

* detail parcel now works with addresses from syndication ([4a24102](https://github.com/informatievlaanderen/parcel-registry/commit/4a24102))

# [1.11.0](https://github.com/informatievlaanderen/parcel-registry/compare/v1.10.0...v1.11.0) (2019-08-19)


### Features

* add wait for input to importer ([7ffe1dd](https://github.com/informatievlaanderen/parcel-registry/commit/7ffe1dd))

# [1.10.0](https://github.com/informatievlaanderen/parcel-registry/compare/v1.9.6...v1.10.0) (2019-08-13)


### Features

* add missing event handlers where nothing was expected [#19](https://github.com/informatievlaanderen/parcel-registry/issues/19) ([fa7ec41](https://github.com/informatievlaanderen/parcel-registry/commit/fa7ec41))

## [1.9.6](https://github.com/informatievlaanderen/parcel-registry/compare/v1.9.5...v1.9.6) (2019-08-09)


### Bug Fixes

* fix container id in logging ([234bc2b](https://github.com/informatievlaanderen/parcel-registry/commit/234bc2b))

## [1.9.5](https://github.com/informatievlaanderen/parcel-registry/compare/v1.9.4...v1.9.5) (2019-08-06)


### Bug Fixes

* save processed capakeys in correct format ([4fb67a1](https://github.com/informatievlaanderen/parcel-registry/commit/4fb67a1))

## [1.9.4](https://github.com/informatievlaanderen/parcel-registry/compare/v1.9.3...v1.9.4) (2019-08-05)


### Bug Fixes

* import with processed keys now serialize correctly ([c5319a7](https://github.com/informatievlaanderen/parcel-registry/commit/c5319a7))

## [1.9.3](https://github.com/informatievlaanderen/parcel-registry/compare/v1.9.2...v1.9.3) (2019-07-17)


### Bug Fixes

* use serilog compact log ([ccdacf0](https://github.com/informatievlaanderen/parcel-registry/commit/ccdacf0))

## [1.9.2](https://github.com/informatievlaanderen/parcel-registry/compare/v1.9.1...v1.9.2) (2019-07-17)


### Bug Fixes

* do not hardcode logging to console ([7dd2ca7](https://github.com/informatievlaanderen/parcel-registry/commit/7dd2ca7))
* do not hardcode logging to console ([5dc7705](https://github.com/informatievlaanderen/parcel-registry/commit/5dc7705))

## [1.9.1](https://github.com/informatievlaanderen/parcel-registry/compare/v1.9.0...v1.9.1) (2019-07-17)


### Bug Fixes

* push syndications container ([0c4166b](https://github.com/informatievlaanderen/parcel-registry/commit/0c4166b))

# [1.9.0](https://github.com/informatievlaanderen/parcel-registry/compare/v1.8.0...v1.9.0) (2019-07-17)


### Features

* add deploy to production ([3e03a65](https://github.com/informatievlaanderen/parcel-registry/commit/3e03a65))

# [1.8.0](https://github.com/informatievlaanderen/parcel-registry/compare/v1.7.0...v1.8.0) (2019-07-10)


### Features

* rename oslo id to persistent local id ([2a36957](https://github.com/informatievlaanderen/parcel-registry/commit/2a36957))

# [1.7.0](https://github.com/informatievlaanderen/parcel-registry/compare/v1.6.0...v1.7.0) (2019-07-10)


### Features

* prepare for deploy ([4286827](https://github.com/informatievlaanderen/parcel-registry/commit/4286827))

# [1.6.0](https://github.com/informatievlaanderen/parcel-registry/compare/v1.5.0...v1.6.0) (2019-06-20)


### Features

* upgrade packages and correct import ([0f938fa](https://github.com/informatievlaanderen/parcel-registry/commit/0f938fa))

# [1.5.0](https://github.com/informatievlaanderen/parcel-registry/compare/v1.4.2...v1.5.0) (2019-06-11)


### Features

* upgrade provenance package Plan -> Reason ([f6333d5](https://github.com/informatievlaanderen/parcel-registry/commit/f6333d5))

## [1.4.2](https://github.com/informatievlaanderen/parcel-registry/compare/v1.4.1...v1.4.2) (2019-06-07)

## [1.4.1](https://github.com/informatievlaanderen/parcel-registry/compare/v1.4.0...v1.4.1) (2019-05-23)


### Bug Fixes

* correct event name dettach => detach ([bb3b3b1](https://github.com/informatievlaanderen/parcel-registry/commit/bb3b3b1))

# [1.4.0](https://github.com/informatievlaanderen/parcel-registry/compare/v1.3.3...v1.4.0) (2019-05-23)


### Features

* add event as xml to sync ([b70cfcd](https://github.com/informatievlaanderen/parcel-registry/commit/b70cfcd))

## [1.3.3](https://github.com/informatievlaanderen/parcel-registry/compare/v1.3.2...v1.3.3) (2019-05-21)

## [1.3.2](https://github.com/informatievlaanderen/parcel-registry/compare/v1.3.1...v1.3.2) (2019-05-20)

## [1.3.1](https://github.com/informatievlaanderen/parcel-registry/compare/v1.3.0...v1.3.1) (2019-05-20)


### Bug Fixes

* stop sync feed last page from crashing ([d450f16](https://github.com/informatievlaanderen/parcel-registry/commit/d450f16))

# [1.3.0](https://github.com/informatievlaanderen/parcel-registry/compare/v1.2.2...v1.3.0) (2019-04-30)


### Features

* add projector + clean up projection libs ([bef69e2](https://github.com/informatievlaanderen/parcel-registry/commit/bef69e2))
* upgrade packages ([d93aabb](https://github.com/informatievlaanderen/parcel-registry/commit/d93aabb))

## [1.2.2](https://github.com/informatievlaanderen/parcel-registry/compare/v1.2.1...v1.2.2) (2019-04-18)

## [1.2.1](https://github.com/informatievlaanderen/parcel-registry/compare/v1.2.0...v1.2.1) (2019-03-04)

# [1.2.0](https://github.com/informatievlaanderen/parcel-registry/compare/v1.1.1...v1.2.0) (2019-03-01)


### Bug Fixes

* add AsNoTracking() to detail parcel ([21c19fa](https://github.com/informatievlaanderen/parcel-registry/commit/21c19fa))
* remove unneeded param and status code from legacy list ([0de86c3](https://github.com/informatievlaanderen/parcel-registry/commit/0de86c3))


### Features

* add address oslo ids to legacy api by syndication ([10c3d64](https://github.com/informatievlaanderen/parcel-registry/commit/10c3d64))
* add migrations syndication ([6365620](https://github.com/informatievlaanderen/parcel-registry/commit/6365620))

## [1.1.1](https://github.com/informatievlaanderen/parcel-registry/compare/v1.1.0...v1.1.1) (2019-03-01)

# [1.1.0](https://github.com/informatievlaanderen/parcel-registry/compare/v1.0.8...v1.1.0) (2019-03-01)


### Features

* add sync ([8fa38a9](https://github.com/informatievlaanderen/parcel-registry/commit/8fa38a9))

## [1.0.8](https://github.com/informatievlaanderen/parcel-registry/compare/v1.0.7...v1.0.8) (2019-02-26)

## [1.0.7](https://github.com/informatievlaanderen/parcel-registry/compare/v1.0.6...v1.0.7) (2019-02-25)

## [1.0.6](https://github.com/informatievlaanderen/parcel-registry/compare/v1.0.5...v1.0.6) (2019-02-25)


### Bug Fixes

* use new lastchangedlist migrations runner ([b22d050](https://github.com/informatievlaanderen/parcel-registry/commit/b22d050))

## [1.0.5](https://github.com/informatievlaanderen/parcel-registry/compare/v1.0.4...v1.0.5) (2019-02-07)


### Bug Fixes

* load the addresses when required ([795d419](https://github.com/informatievlaanderen/parcel-registry/commit/795d419))
* null ref when handling address in detail projection ([a4263f6](https://github.com/informatievlaanderen/parcel-registry/commit/a4263f6))

## [1.0.4](https://github.com/informatievlaanderen/parcel-registry/compare/v1.0.3...v1.0.4) (2019-02-07)


### Bug Fixes

* support nullable Rfc3339SerializableDateTimeOffset in converter ([dd3d1c1](https://github.com/informatievlaanderen/parcel-registry/commit/dd3d1c1))

## [1.0.3](https://github.com/informatievlaanderen/parcel-registry/compare/v1.0.2...v1.0.3) (2019-02-06)


### Bug Fixes

* properly serialise rfc 3339 dates ([a6c5003](https://github.com/informatievlaanderen/parcel-registry/commit/a6c5003))

## [1.0.2](https://github.com/informatievlaanderen/parcel-registry/compare/v1.0.1...v1.0.2) (2019-01-24)

## [1.0.1](https://github.com/informatievlaanderen/parcel-registry/compare/v1.0.0...v1.0.1) (2019-01-24)


### Bug Fixes

* use basic auth for importer ([23fb49d](https://github.com/informatievlaanderen/parcel-registry/commit/23fb49d))

# 1.0.0 (2019-01-22)


### Features

* open source with EUPL-1.2 license as 'agentschap Informatie Vlaanderen' ([5ddef9f](https://github.com/informatievlaanderen/parcel-registry/commit/5ddef9f))
