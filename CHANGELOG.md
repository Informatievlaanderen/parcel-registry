## [4.31.12](https://github.com/informatievlaanderen/parcel-registry/compare/v4.31.11...v4.31.12) (2023-09-07)


### Bug Fixes

* **importer:** correct from date to last used date GAWR-5189 ([6e5c37c](https://github.com/informatievlaanderen/parcel-registry/commit/6e5c37c4a1e28e90ebf968744de3d0d76927d4a1))

## [4.31.11](https://github.com/informatievlaanderen/parcel-registry/compare/v4.31.10...v4.31.11) (2023-09-05)


### Bug Fixes

* event descriptions ([b134ec2](https://github.com/informatievlaanderen/parcel-registry/commit/b134ec28658280e07023bfa3f65ef89a90f867b3))
* migrated parcels are always realized ([aabe4ca](https://github.com/informatievlaanderen/parcel-registry/commit/aabe4cab9230eea188a6f8c7470f57e290dc217b))

## [4.31.10](https://github.com/informatievlaanderen/parcel-registry/compare/v4.31.9...v4.31.10) (2023-09-05)


### Bug Fixes

* correct cachekey last changed list ([b4d4805](https://github.com/informatievlaanderen/parcel-registry/commit/b4d4805f7981012ee002bbffd734b971fab2b34c))

## [4.31.9](https://github.com/informatievlaanderen/parcel-registry/compare/v4.31.8...v4.31.9) (2023-09-04)


### Bug Fixes

* bump lambda package ([7882cab](https://github.com/informatievlaanderen/parcel-registry/commit/7882cab185cbda22dadf035917c43232b6993604))
* bump lambda packages ([8b1757f](https://github.com/informatievlaanderen/parcel-registry/commit/8b1757fffe8d1498eba4a47e95320feda3e2ea96))
* naming status import grb ([f68fea6](https://github.com/informatievlaanderen/parcel-registry/commit/f68fea66bb1102a630ef8708cf2938b3d81f32ad))

## [4.31.8](https://github.com/informatievlaanderen/parcel-registry/compare/v4.31.7...v4.31.8) (2023-08-30)


### Bug Fixes

* handle ParcelWasMigrated in backoffice projections ([b1865ab](https://github.com/informatievlaanderen/parcel-registry/commit/b1865ab8cec4f227d1afa3cf0837dbe6fd8e1b55))

## [4.31.7](https://github.com/informatievlaanderen/parcel-registry/compare/v4.31.6...v4.31.7) (2023-08-30)


### Bug Fixes

* migrate command has unique address ids ([dd4a22f](https://github.com/informatievlaanderen/parcel-registry/commit/dd4a22f2ac4acd7c59c6d9cdf54542327318cabc))
* parcel importer should only run when from date is greater than to date ([0e3f052](https://github.com/informatievlaanderen/parcel-registry/commit/0e3f05263b8e9bf10480dcd73e7fb1d311cab419))

## [4.31.6](https://github.com/informatievlaanderen/parcel-registry/compare/v4.31.5...v4.31.6) (2023-08-22)


### Bug Fixes

* throw orderInvalidDateRangeException + notify slack of completed run ([98697da](https://github.com/informatievlaanderen/parcel-registry/commit/98697da231d904faa78e809bef9841c7f689dc1b))

## [4.31.5](https://github.com/informatievlaanderen/parcel-registry/compare/v4.31.4...v4.31.5) (2023-08-21)


### Bug Fixes

* add grar notifications package ([76d53d3](https://github.com/informatievlaanderen/parcel-registry/commit/76d53d32844b00884b65d065edc838d607a64e97))

## [4.31.4](https://github.com/informatievlaanderen/parcel-registry/compare/v4.31.3...v4.31.4) (2023-08-21)


### Bug Fixes

* call cancel on lambda cancellationToken after 5 minutes ([e7b8494](https://github.com/informatievlaanderen/parcel-registry/commit/e7b8494d4d4cc65250ea8f6bb88e45206049a988))

## [4.31.3](https://github.com/informatievlaanderen/parcel-registry/compare/v4.31.2...v4.31.3) (2023-08-17)


### Bug Fixes

* how truncate processedRequests is called on importerContext ([73f06f4](https://github.com/informatievlaanderen/parcel-registry/commit/73f06f462f839723e3cc4959a8401189e964ba7d))

## [4.31.2](https://github.com/informatievlaanderen/parcel-registry/compare/v4.31.1...v4.31.2) (2023-08-17)


### Bug Fixes

* use (toDate + 1day) from last completed run as new fromDate ([edbd672](https://github.com/informatievlaanderen/parcel-registry/commit/edbd67239fb14243f2ae852b4a4cd24b04b14580))

## [4.31.1](https://github.com/informatievlaanderen/parcel-registry/compare/v4.31.0...v4.31.1) (2023-08-16)


### Bug Fixes

* stop application after processing ([fbe2fad](https://github.com/informatievlaanderen/parcel-registry/commit/fbe2fad39c34ee0befd2f86434656728fd6af3c4))


### Performance Improvements

* use truncate to clear processed requests ([5b5db0d](https://github.com/informatievlaanderen/parcel-registry/commit/5b5db0df29817a032256d909028565e9f94df158))

# [4.31.0](https://github.com/informatievlaanderen/parcel-registry/compare/v4.30.5...v4.31.0) (2023-08-14)


### Features

* expand ChangeParcelGeometry to import when parcel doesn't exist ([b1027c8](https://github.com/informatievlaanderen/parcel-registry/commit/b1027c8f5d37f85cbc9afaa68d827e8d6eaf46c8))

## [4.30.5](https://github.com/informatievlaanderen/parcel-registry/compare/v4.30.4...v4.30.5) (2023-08-11)


### Bug Fixes

* fix the geometry before sending to SqlServer ([2ac81fd](https://github.com/informatievlaanderen/parcel-registry/commit/2ac81fddf8eea338416a6c110ea64dd2fc9a09d6))

## [4.30.4](https://github.com/informatievlaanderen/parcel-registry/compare/v4.30.3...v4.30.4) (2023-08-10)


### Bug Fixes

* split contains & touches methods in spatial query for performance ([36a5f92](https://github.com/informatievlaanderen/parcel-registry/commit/36a5f92211a4b5c325face32db8a34d26c880de7))

## [4.30.3](https://github.com/informatievlaanderen/parcel-registry/compare/v4.30.2...v4.30.3) (2023-08-10)


### Bug Fixes

* route status importer grb ([18035a6](https://github.com/informatievlaanderen/parcel-registry/commit/18035a60b39adf7b95abb4892a82090ebd1c52a6))

## [4.30.2](https://github.com/informatievlaanderen/parcel-registry/compare/v4.30.1...v4.30.2) (2023-08-09)


### Bug Fixes

* importgrb status ([0929723](https://github.com/informatievlaanderen/parcel-registry/commit/092972397a01faa38812ff6e4ffb2e3935e7204a))
* update ci prepare lambda test ([7274469](https://github.com/informatievlaanderen/parcel-registry/commit/7274469d65639b78c14673154fa961f00e100a7e))

## [4.30.1](https://github.com/informatievlaanderen/parcel-registry/compare/v4.30.0...v4.30.1) (2023-08-09)


### Bug Fixes

* prepare lambda to test step ([955d9c9](https://github.com/informatievlaanderen/parcel-registry/commit/955d9c93615d0f30791a6e2fee97882decc1ef40))
* remove appsettings from ConsumerAddress csproj ([1da33a1](https://github.com/informatievlaanderen/parcel-registry/commit/1da33a1ff95aa030f86d2606b31304bbe75691e8))

# [4.30.0](https://github.com/informatievlaanderen/parcel-registry/compare/v4.29.0...v4.30.0) (2023-08-09)


### Bug Fixes

* change release for lambda on test ([d9f2992](https://github.com/informatievlaanderen/parcel-registry/commit/d9f2992951324960e0b4ecaa2c96bc2d2b55e0be))


### Features

* add importer grb status ([014e81d](https://github.com/informatievlaanderen/parcel-registry/commit/014e81dce5e87a24cdedcf29d0f7e269b353d2e2))
* split AddressConsumer into console & lib projects ([9a6faaa](https://github.com/informatievlaanderen/parcel-registry/commit/9a6faaa2c99b3e350975067ed8e93f0cf36ef287))

# [4.29.0](https://github.com/informatievlaanderen/parcel-registry/compare/v4.28.1...v4.29.0) (2023-08-09)


### Bug Fixes

* remove ErrorOnDuplicatePublishOutputFiles on address consumer ([5c8b44a](https://github.com/informatievlaanderen/parcel-registry/commit/5c8b44a9f3506b370ab0a46382183648202d813d))


### Features

* add GRB_01_PARCEL_SNAPSHOT_OSLO_STREAM_FLATTEN.ksql ([62bec4f](https://github.com/informatievlaanderen/parcel-registry/commit/62bec4f0429edf45332fbc61d3765a445daeea27))

## [4.28.1](https://github.com/informatievlaanderen/parcel-registry/compare/v4.28.0...v4.28.1) (2023-08-01)


### Bug Fixes

* new release because of init.sh overwrite problem ([efc5c52](https://github.com/informatievlaanderen/parcel-registry/commit/efc5c52af3cc932578cdd66574b173b504723eda))

# [4.28.0](https://github.com/informatievlaanderen/parcel-registry/compare/v4.27.9...v4.28.0) (2023-08-01)


### Features

* add ParcelRetirementWasCorrected ([9460df7](https://github.com/informatievlaanderen/parcel-registry/commit/9460df7684b826864f777a4f658556794c7d7d3e))
* add projections for ParcelWasCorrectedFromRetiredToRealized ([e9614aa](https://github.com/informatievlaanderen/parcel-registry/commit/e9614aa49a20735a42d6938c9b810402a556a7dc))

## [4.27.9](https://github.com/informatievlaanderen/parcel-registry/compare/v4.27.8...v4.27.9) (2023-07-28)


### Bug Fixes

* commandhandler lifetime exception ([d525ba0](https://github.com/informatievlaanderen/parcel-registry/commit/d525ba0495b0776d12be29fe8cd25768b3e2ce7d))
* order on version date then on version ([4839c4a](https://github.com/informatievlaanderen/parcel-registry/commit/4839c4a1cee49ddb4881bcfdbbc3b7dabdb2300c))

## [4.27.8](https://github.com/informatievlaanderen/parcel-registry/compare/v4.27.7...v4.27.8) (2023-07-27)


### Bug Fixes

* double registration of importer + sql timeout to 120 sec ([140570b](https://github.com/informatievlaanderen/parcel-registry/commit/140570b29bd3eca09f84fa96f53560beeee63ae8))

## [4.27.7](https://github.com/informatievlaanderen/parcel-registry/compare/v4.27.6...v4.27.7) (2023-07-26)


### Bug Fixes

* dummy to trigger release ([22402ff](https://github.com/informatievlaanderen/parcel-registry/commit/22402fff1628762273fa5d641d047fc52eb1f0d9))

## [4.27.6](https://github.com/informatievlaanderen/parcel-registry/compare/v4.27.5...v4.27.6) (2023-07-25)


### Bug Fixes

* add snapshot registration to importer ([cf7e167](https://github.com/informatievlaanderen/parcel-registry/commit/cf7e1679f7d7aafb254eccb1e98e5a042d4b8c6c))

## [4.27.5](https://github.com/informatievlaanderen/parcel-registry/compare/v4.27.4...v4.27.5) (2023-07-25)


### Bug Fixes

* spatial query typo ([1c9f4c5](https://github.com/informatievlaanderen/parcel-registry/commit/1c9f4c50a6e6ebe60362a1dc81e11e3f05455042))

## [4.27.4](https://github.com/informatievlaanderen/parcel-registry/compare/v4.27.3...v4.27.4) (2023-07-25)


### Bug Fixes

* dummy commit for release ([9bcaeb0](https://github.com/informatievlaanderen/parcel-registry/commit/9bcaeb082460ac07c94788cc8c97a2fabbc0330a))

## [4.27.3](https://github.com/informatievlaanderen/parcel-registry/compare/v4.27.2...v4.27.3) (2023-07-24)


### Bug Fixes

* re-make grb_download_file.zip ([703eea9](https://github.com/informatievlaanderen/parcel-registry/commit/703eea94bb0dbdce1bf2e603710c63c8193ef015))
* zipArchiveProcessor read entry file path ([588a9ed](https://github.com/informatievlaanderen/parcel-registry/commit/588a9ed2dc5238c0d02643db60b5c5730028b7c3))

## [4.27.2](https://github.com/informatievlaanderen/parcel-registry/compare/v4.27.1...v4.27.2) (2023-07-20)


### Bug Fixes

* register notification service in importer ([ff296b6](https://github.com/informatievlaanderen/parcel-registry/commit/ff296b68129108bc43ca836b402f1b7d873fe829))

## [4.27.1](https://github.com/informatievlaanderen/parcel-registry/compare/v4.27.0...v4.27.1) (2023-07-20)


### Bug Fixes

* add ConsumerAddress connectionstring to importer ([1e70d6d](https://github.com/informatievlaanderen/parcel-registry/commit/1e70d6d8b45f7f41af94151624db2f1d624b8b5f))
* remove index creation because its already made ([aa4e1c4](https://github.com/informatievlaanderen/parcel-registry/commit/aa4e1c400339d3a4999d1901e397f7b651d49c8a))

# [4.27.0](https://github.com/informatievlaanderen/parcel-registry/compare/v4.26.0...v4.27.0) (2023-07-18)


### Features

* perform spatial query to collect addresses within a parcel ([78efd01](https://github.com/informatievlaanderen/parcel-registry/commit/78efd01ffea46177df96291e1357ce1c2d7cfaa5))

# [4.26.0](https://github.com/informatievlaanderen/parcel-registry/compare/v4.25.0...v4.26.0) (2023-07-17)


### Bug Fixes

* add release for grb importer ([f965719](https://github.com/informatievlaanderen/parcel-registry/commit/f96571900d13017375cab845ca73017e690d5b6d))
* add spatial index on consumer address ([d78bd3e](https://github.com/informatievlaanderen/parcel-registry/commit/d78bd3eba3c6eb78b6c30b8d8947b876e10ec1e3))


### Features

* add importer downloadclient ([55f7ff7](https://github.com/informatievlaanderen/parcel-registry/commit/55f7ff760c27ffd3b583a9ce2411ae82f21de92e))
* attach/detach addresses to import/change geometry in domain ([bf3793e](https://github.com/informatievlaanderen/parcel-registry/commit/bf3793e90d5fbb2acb523b5005d04b56a10cd6ee))
* import grb data ([6b88c74](https://github.com/informatievlaanderen/parcel-registry/commit/6b88c7434473361392b84c33cbf03be7e35f8c69))

# [4.25.0](https://github.com/informatievlaanderen/parcel-registry/compare/v4.24.0...v4.25.0) (2023-07-10)


### Bug Fixes

* don't cancel migrator after certain point ([58e281a](https://github.com/informatievlaanderen/parcel-registry/commit/58e281ab9f4d4044fa3752d330a612e875dc9c92))


### Features

* add version to GrbParcel ([2030632](https://github.com/informatievlaanderen/parcel-registry/commit/2030632ad1d0f4ba0a59ccbe5a33a6297bd336f0))
* change parcel geometry ([3666205](https://github.com/informatievlaanderen/parcel-registry/commit/3666205d1dcf2237e6e3b4fd70507ab154f66e37))

# [4.24.0](https://github.com/informatievlaanderen/parcel-registry/compare/v4.23.0...v4.24.0) (2023-07-10)


### Bug Fixes

* enumerate addresses when retiring legacy parcel ([652e57e](https://github.com/informatievlaanderen/parcel-registry/commit/652e57e3314fe8b2f48e5ce6c20840c02acac8cd))


### Features

* retire parcel ([6f6a019](https://github.com/informatievlaanderen/parcel-registry/commit/6f6a0194f21329bbce4e7ab93e701293a935a959))

# [4.23.0](https://github.com/informatievlaanderen/parcel-registry/compare/v4.22.1...v4.23.0) (2023-07-07)


### Features

* add geometry to projections ([4d830f7](https://github.com/informatievlaanderen/parcel-registry/commit/4d830f70cb213e303d047053802ba349ba621d30))

## [4.22.1](https://github.com/informatievlaanderen/parcel-registry/compare/v4.22.0...v4.22.1) (2023-07-07)


### Bug Fixes

* remove address ids when retiring parcel ([1fb5ec6](https://github.com/informatievlaanderen/parcel-registry/commit/1fb5ec6d37e415ac3a76bc097d13af8f7a9f6c5d))
* use NTS for EF in consumer address DbContextFactory ([f357b26](https://github.com/informatievlaanderen/parcel-registry/commit/f357b26667655deb049853c4f32d22dc69bfeaeb))

# [4.22.0](https://github.com/informatievlaanderen/parcel-registry/compare/v4.21.0...v4.22.0) (2023-07-07)


### Bug Fixes

* importparcels split streamid ([2a68ae8](https://github.com/informatievlaanderen/parcel-registry/commit/2a68ae8cec1509252af4d4d59c9b8e302bf91276))
* use NTS for EF in consumer address ([b9897cb](https://github.com/informatievlaanderen/parcel-registry/commit/b9897cb2378bab84205a5cef985a7a5318b80288))


### Features

* add extendedWkbGeometry to migrate parcel + import parcel ([ce91fff](https://github.com/informatievlaanderen/parcel-registry/commit/ce91fff5ae36afaf2639b91337bcc040d3e8c045))
* add Grb XML Readers ([91ab074](https://github.com/informatievlaanderen/parcel-registry/commit/91ab074cd368a4dd2bbb11c475d6d592a6c5ab3f))
* add Importer Grb skeleton with XmlReaders ([10a9668](https://github.com/informatievlaanderen/parcel-registry/commit/10a966805238401398f28730316428503ad1483a))
* add importing new parcels after migration ([37a2e52](https://github.com/informatievlaanderen/parcel-registry/commit/37a2e5265546d7e960177a77a837c40fae8d44cc))
* read parcel geometries and migrate or retire old Parcels ([43ae62e](https://github.com/informatievlaanderen/parcel-registry/commit/43ae62e3e35036281be8a2bbde0c8ee997ad43be))

# [4.21.0](https://github.com/informatievlaanderen/parcel-registry/compare/v4.20.6...v4.21.0) (2023-06-23)


### Features

* consume address geometry ([d0d9f81](https://github.com/informatievlaanderen/parcel-registry/commit/d0d9f817548cbe576cd154f662c87f8dd1dab323))

## [4.20.6](https://github.com/informatievlaanderen/parcel-registry/compare/v4.20.5...v4.20.6) (2023-06-22)


### Bug Fixes

* bump projectionhandling ([a136e7c](https://github.com/informatievlaanderen/parcel-registry/commit/a136e7c264102b1e3208071c8f318bf0eefda15d))

## [4.20.5](https://github.com/informatievlaanderen/parcel-registry/compare/v4.20.4...v4.20.5) (2023-06-21)


### Bug Fixes

* naming producers migration ([be28549](https://github.com/informatievlaanderen/parcel-registry/commit/be28549d9ed0cec65469eb6a55a523d8118c4697))

## [4.20.4](https://github.com/informatievlaanderen/parcel-registry/compare/v4.20.3...v4.20.4) (2023-06-16)


### Bug Fixes

* also use StatusAsString in legacy api v2 query ([19c9039](https://github.com/informatievlaanderen/parcel-registry/commit/19c9039d6bc9de470706bdea05dda67b2f5e607c))


### Performance Improvements

* read addresses from legacy instead of consumer ([6e2b193](https://github.com/informatievlaanderen/parcel-registry/commit/6e2b1933542d3e157744293a34f6cb2ed28c94f4))

## [4.20.3](https://github.com/informatievlaanderen/parcel-registry/compare/v4.20.2...v4.20.3) (2023-06-14)


### Bug Fixes

* add StatusAsString to ParceDetailV2 to fix filtering + remake CaPaKey index ([1772d85](https://github.com/informatievlaanderen/parcel-registry/commit/1772d851b126d2f87b61245974730b491945457d))

## [4.20.2](https://github.com/informatievlaanderen/parcel-registry/compare/v4.20.1...v4.20.2) (2023-06-14)


### Bug Fixes

* producer naming ([d9bddd3](https://github.com/informatievlaanderen/parcel-registry/commit/d9bddd3d17153a6591e4a2b75f7d7a97445d4972))

## [4.20.1](https://github.com/informatievlaanderen/parcel-registry/compare/v4.20.0...v4.20.1) (2023-06-13)


### Bug Fixes

* add idempotent address-parcel relation ([f9bd59c](https://github.com/informatievlaanderen/parcel-registry/commit/f9bd59c72d92c4d3898da35bdf3538bff6b911cd))

# [4.20.0](https://github.com/informatievlaanderen/parcel-registry/compare/v4.19.0...v4.20.0) (2023-06-12)


### Features

* add consumer controller ([b80208c](https://github.com/informatievlaanderen/parcel-registry/commit/b80208c3d6530b7a54ced2819fc3f0849bb0cce0))

# [4.19.0](https://github.com/informatievlaanderen/parcel-registry/compare/v4.18.7...v4.19.0) (2023-06-07)


### Features

* get parcel adresses from crab export instead of legacy aggregate ([ef6e330](https://github.com/informatievlaanderen/parcel-registry/commit/ef6e330a255e242276d67adcbb104afb41090c4c))

## [4.18.7](https://github.com/informatievlaanderen/parcel-registry/compare/v4.18.6...v4.18.7) (2023-06-01)


### Bug Fixes

* to trigger build cd newprd ([038c747](https://github.com/informatievlaanderen/parcel-registry/commit/038c747b52eb92a5df2573bdb79f65dc86e23534))

## [4.18.6](https://github.com/informatievlaanderen/parcel-registry/compare/v4.18.5...v4.18.6) (2023-06-01)


### Bug Fixes

* ci / cd newprd fixes ([7b7ac1d](https://github.com/informatievlaanderen/parcel-registry/commit/7b7ac1d6239a39896f5ab639ef763be01339db0f))

## [4.18.5](https://github.com/informatievlaanderen/parcel-registry/compare/v4.18.4...v4.18.5) (2023-06-01)


### Bug Fixes

* ci/cd new prd fixes ([4d582ab](https://github.com/informatievlaanderen/parcel-registry/commit/4d582ab0cda422b3c1514792847305db195017fc))

## [4.18.4](https://github.com/informatievlaanderen/parcel-registry/compare/v4.18.3...v4.18.4) (2023-06-01)


### Bug Fixes

* get consumerItems before processing migrator ([d327c01](https://github.com/informatievlaanderen/parcel-registry/commit/d327c0192156b351fb15ce4baa870077c9f8ae81))
* to trigger build add ci/cd new prd ([617b541](https://github.com/informatievlaanderen/parcel-registry/commit/617b541d42b94cafae024d2bf0a0d158f290ab49))


### Performance Improvements

* run migrator in parallel ([b73f64f](https://github.com/informatievlaanderen/parcel-registry/commit/b73f64fc40f7207f7973b76287d7dee109a327c1))

## [4.18.3](https://github.com/informatievlaanderen/parcel-registry/compare/v4.18.2...v4.18.3) (2023-05-26)


### Bug Fixes

* change name status page parcel address projections ([09af1ca](https://github.com/informatievlaanderen/parcel-registry/commit/09af1ca7166abd9f6d74be7b0fb522f0542d0c4f))

## [4.18.2](https://github.com/informatievlaanderen/parcel-registry/compare/v4.18.1...v4.18.2) (2023-05-10)


### Bug Fixes

* add snapshotmanager with etag comparison ([fb2b2cc](https://github.com/informatievlaanderen/parcel-registry/commit/fb2b2cc4d9a87e2914e005eb227e1fe5bc8ed237))

## [4.18.1](https://github.com/informatievlaanderen/parcel-registry/compare/v4.18.0...v4.18.1) (2023-05-04)


### Bug Fixes

* change docs event readdress ([15ec76b](https://github.com/informatievlaanderen/parcel-registry/commit/15ec76b9dd209da914d1702eee231ce33552bb0a))

# [4.18.0](https://github.com/informatievlaanderen/parcel-registry/compare/v4.17.2...v4.18.0) (2023-04-18)


### Features

* consume new readdress events ([303927b](https://github.com/informatievlaanderen/parcel-registry/commit/303927b4877e747829a8fe3a2eff478b6413138a))

## [4.17.2](https://github.com/informatievlaanderen/parcel-registry/compare/v4.17.1...v4.17.2) (2023-04-13)

## [4.17.1](https://github.com/informatievlaanderen/parcel-registry/compare/v4.17.0...v4.17.1) (2023-04-11)


### Bug Fixes

* ParcelAddressWasReplacedBecauseAddressWasReaddressed ([e0150dc](https://github.com/informatievlaanderen/parcel-registry/commit/e0150dc648efe83399ca92f903e307c9a2fa3560))
* ReplaceAttachedAddressBecauseAddressWasReaddressed ([1d61086](https://github.com/informatievlaanderen/parcel-registry/commit/1d6108679d8122f0c76b4324e4806c64b68e01b1))

# [4.17.0](https://github.com/informatievlaanderen/parcel-registry/compare/v4.16.0...v4.17.0) (2023-04-07)


### Bug Fixes

* don't extract removed parcels in addresslink extract ([9ac6549](https://github.com/informatievlaanderen/parcel-registry/commit/9ac6549d2cbde7afb010d3eff9e6aa10e6d92492))
* run containers as non-root ([20c7fc8](https://github.com/informatievlaanderen/parcel-registry/commit/20c7fc880c5ea0bc999fbef8bb8514609507ba86))


### Features

* consume AddressHouseNumberWasReaddressed ([95a57fb](https://github.com/informatievlaanderen/parcel-registry/commit/95a57fbf477bab322405016d7aa10ef5bb06c78b))

# [4.16.0](https://github.com/informatievlaanderen/parcel-registry/compare/v4.15.3...v4.16.0) (2023-04-03)


### Features

* add parcel-address links extract ([7e04076](https://github.com/informatievlaanderen/parcel-registry/commit/7e04076fb83cb7e2904639c30f3c4b2fa806fdf6))

## [4.15.3](https://github.com/informatievlaanderen/parcel-registry/compare/v4.15.2...v4.15.3) (2023-03-30)


### Bug Fixes

* use snapshotstore and add tests ([60c7499](https://github.com/informatievlaanderen/parcel-registry/commit/60c749902a4ca84634ee01fb8eb15ac55e5b6173))

## [4.15.2](https://github.com/informatievlaanderen/parcel-registry/compare/v4.15.1...v4.15.2) (2023-03-13)

## [4.15.1](https://github.com/informatievlaanderen/parcel-registry/compare/v4.15.0...v4.15.1) (2023-03-07)


### Bug Fixes

* make producer reliable ([c680d58](https://github.com/informatievlaanderen/parcel-registry/commit/c680d58db8c34666bfa35b8987510cba645cdd5d))

# [4.15.0](https://github.com/informatievlaanderen/parcel-registry/compare/v4.14.1...v4.15.0) (2023-03-06)


### Features

* detach address when address is removed because streetname was removed ([370c213](https://github.com/informatievlaanderen/parcel-registry/commit/370c213cb00c86a59ab2def13ce4f8c731e7b419))

## [4.14.1](https://github.com/informatievlaanderen/parcel-registry/compare/v4.14.0...v4.14.1) (2023-03-01)


### Bug Fixes

* response examples ([5c37ae7](https://github.com/informatievlaanderen/parcel-registry/commit/5c37ae7cc32546eb74736d3880ee325cc3bf169f))

# [4.14.0](https://github.com/informatievlaanderen/parcel-registry/compare/v4.13.3...v4.14.0) (2023-03-01)


### Bug Fixes

* no merge group for ksql ([208e149](https://github.com/informatievlaanderen/parcel-registry/commit/208e149607f8eb1eaefbd64c79ce85796349ec7e))


### Features

* add v2 example ([5c0fcd3](https://github.com/informatievlaanderen/parcel-registry/commit/5c0fcd30423f9c61593ea3dfaf2de734f153e173))

## [4.13.3](https://github.com/informatievlaanderen/parcel-registry/compare/v4.13.2...v4.13.3) (2023-02-27)


### Bug Fixes

* bump grar common to 18.1.1 ([8ff025d](https://github.com/informatievlaanderen/parcel-registry/commit/8ff025d60c90efe33610eecfa19fc0157a972929))
* bump mediatr ([1805818](https://github.com/informatievlaanderen/parcel-registry/commit/1805818c43161cf1ce1185d1f171b5fae24e05bb))

## [4.13.2](https://github.com/informatievlaanderen/parcel-registry/compare/v4.13.1...v4.13.2) (2023-02-23)


### Bug Fixes

* correct lambda bucket name in CI ([c901143](https://github.com/informatievlaanderen/parcel-registry/commit/c901143539b64de38ef3d01c34ec95d91673014d))

## [4.13.1](https://github.com/informatievlaanderen/parcel-registry/compare/v4.13.0...v4.13.1) (2023-02-23)


### Bug Fixes

* use new ci flow ([4468342](https://github.com/informatievlaanderen/parcel-registry/commit/4468342f275e90bb271c1f527a74158f61958454))

# [4.13.0](https://github.com/informatievlaanderen/parcel-registry/compare/v4.12.5...v4.13.0) (2023-02-21)


### Bug Fixes

* consumer should commit if message is already processed ([e6143fd](https://github.com/informatievlaanderen/parcel-registry/commit/e6143fd2eb1b5c59b069e158d92653e6b3bdc1ef))
* number ksql ([f72bd14](https://github.com/informatievlaanderen/parcel-registry/commit/f72bd1413adea9f2521ba3d48afb4f2f7df89f1a))
* use merge queue ([53de53b](https://github.com/informatievlaanderen/parcel-registry/commit/53de53b7bb8f11e97a1b3dad5d3ad44cda33955c))


### Features

* consume address rejection by streetname events for backoffice projection ([fe62f0f](https://github.com/informatievlaanderen/parcel-registry/commit/fe62f0faedccbca30a9b0e0f1ad8032ec9b0a510))

## [4.12.5](https://github.com/informatievlaanderen/parcel-registry/compare/v4.12.4...v4.12.5) (2023-02-13)


### Bug Fixes

* event jsondata should contain provenance ([7297e1e](https://github.com/informatievlaanderen/parcel-registry/commit/7297e1e54aabf43662f690ab586e5442fa5b3dee))
* ksql msgkey to int ([b57287a](https://github.com/informatievlaanderen/parcel-registry/commit/b57287a2bd48095e792a86b7d01406ff71aa38db))
* use correct date in provenance for consumer command handling ([81228e5](https://github.com/informatievlaanderen/parcel-registry/commit/81228e58eaf8d464db7304dd1676c67c85539a4a))

## [4.12.4](https://github.com/informatievlaanderen/parcel-registry/compare/v4.12.3...v4.12.4) (2023-01-26)


### Bug Fixes

* add uow registration with aggregatesource ([b3b83b1](https://github.com/informatievlaanderen/parcel-registry/commit/b3b83b13a2329afb4c4c080ef74185dee11b0a40))

## [4.12.3](https://github.com/informatievlaanderen/parcel-registry/compare/v4.12.2...v4.12.3) (2023-01-26)


### Bug Fixes

* remove dockerfile init.sh ([19a21d0](https://github.com/informatievlaanderen/parcel-registry/commit/19a21d07b6793d9954d6f050f5bef0b83736a2aa))

## [4.12.2](https://github.com/informatievlaanderen/parcel-registry/compare/v4.12.1...v4.12.2) (2023-01-25)


### Bug Fixes

* cleanup paket.references ([76ca8b3](https://github.com/informatievlaanderen/parcel-registry/commit/76ca8b35e29133c3b1a6b44cfa98c0ba6293cb81))
* review ioc registrations ([3e86224](https://github.com/informatievlaanderen/parcel-registry/commit/3e86224f609d2491b0fd7bee58376dd12783c19c))

## [4.12.1](https://github.com/informatievlaanderen/parcel-registry/compare/v4.12.0...v4.12.1) (2023-01-24)


### Bug Fixes

* null reference exception at crabimport startup ([4ba33dd](https://github.com/informatievlaanderen/parcel-registry/commit/4ba33ddd3fe3ee08a3a5bf8d228f94cd91b4d888))

# [4.12.0](https://github.com/informatievlaanderen/parcel-registry/compare/v4.11.2...v4.12.0) (2023-01-23)


### Bug Fixes

* change encoding docker file ([5ccc2d9](https://github.com/informatievlaanderen/parcel-registry/commit/5ccc2d9c81bf0dcc325754e8299bd7eda0f47294))


### Features

* add acm/idm ([d7e7208](https://github.com/informatievlaanderen/parcel-registry/commit/d7e720862556d9db163097250857b14c14f73359))
* add acmidm  integrationtests ([dd8a358](https://github.com/informatievlaanderen/parcel-registry/commit/dd8a3581b98bc14bf5d711acc4c1171ba94303ab))

## [4.11.2](https://github.com/informatievlaanderen/parcel-registry/compare/v4.11.1...v4.11.2) (2023-01-13)


### Bug Fixes

* add backoffice projections to cd ([396fea0](https://github.com/informatievlaanderen/parcel-registry/commit/396fea03e1fbfbf329af0acb378e3d8324f633bb))

## [4.11.1](https://github.com/informatievlaanderen/parcel-registry/compare/v4.11.0...v4.11.1) (2023-01-13)


### Bug Fixes

* extract parcel v2 ([11d4d6d](https://github.com/informatievlaanderen/parcel-registry/commit/11d4d6d578092eac9b71103a250c1a9e6d65a9d8))

# [4.11.0](https://github.com/informatievlaanderen/parcel-registry/compare/v4.10.0...v4.11.0) (2023-01-12)


### Features

* update Be.Vlaanderen.Basisregisters.Api to 19.0.1 ([7999c9e](https://github.com/informatievlaanderen/parcel-registry/commit/7999c9e73804882fa179c542f1b5edb92081d9da))

# [4.10.0](https://github.com/informatievlaanderen/parcel-registry/compare/v4.9.2...v4.10.0) (2023-01-10)


### Bug Fixes

* add edit tags to events ([df56f2b](https://github.com/informatievlaanderen/parcel-registry/commit/df56f2b0e9d1a9b665ef70487c92c357f0d3746c))
* change eventpropertydescription for addresspersistentlocalid ([e734ae0](https://github.com/informatievlaanderen/parcel-registry/commit/e734ae0c7267f2865ddf78c35b473afe1055c3ef))


### Features

* projections backoffice ([2df1d4a](https://github.com/informatievlaanderen/parcel-registry/commit/2df1d4ac2b0c2ae5f97841dc3751c41cfb0ea0a7))

## [4.9.2](https://github.com/informatievlaanderen/parcel-registry/compare/v4.9.1...v4.9.2) (2023-01-03)


### Bug Fixes

* trigger release ([a1dcbd0](https://github.com/informatievlaanderen/parcel-registry/commit/a1dcbd0dfe6d8ba46da040bc8af30e53a7e3bbad))

## [4.9.1](https://github.com/informatievlaanderen/parcel-registry/compare/v4.9.0...v4.9.1) (2023-01-03)


### Bug Fixes

* package updates ([c2857d8](https://github.com/informatievlaanderen/parcel-registry/commit/c2857d81a353b197d01792e8155d3c7b79f66d32))

# [4.9.0](https://github.com/informatievlaanderen/parcel-registry/compare/v4.8.6...v4.9.0) (2023-01-03)


### Bug Fixes

* add paket.references to fix build ([8cd9ba4](https://github.com/informatievlaanderen/parcel-registry/commit/8cd9ba4e2e85685c05767734762ab68748e75429))


### Features

* add oslo snapshot producer ([aa7f4a7](https://github.com/informatievlaanderen/parcel-registry/commit/aa7f4a764f14582969b21ca18ee7967fc68a4169))
* add producers ([687c79a](https://github.com/informatievlaanderen/parcel-registry/commit/687c79aa484ec2b3bb8c44ad5ced49a18637334b))

## [4.8.6](https://github.com/informatievlaanderen/parcel-registry/compare/v4.8.5...v4.8.6) (2022-12-21)


### Bug Fixes

* consumer address commandhandling to remove relation from backoffice ([a78b016](https://github.com/informatievlaanderen/parcel-registry/commit/a78b016923d6afe76e2cd7d8c98e236905486652))
* skip addresses removed / invalid status ([73dcd0f](https://github.com/informatievlaanderen/parcel-registry/commit/73dcd0ffae8fe868bba6fa1f0a751b842a2f04cd))

## [4.8.5](https://github.com/informatievlaanderen/parcel-registry/compare/v4.8.4...v4.8.5) (2022-12-19)


### Bug Fixes

* idempotent remove of ParcelAddressRelation ([fb1c835](https://github.com/informatievlaanderen/parcel-registry/commit/fb1c835d687fb9cd06add88f0c048c83383aff6c))
* prevent duplicate back office ParcelAddressRelations ([b037b1c](https://github.com/informatievlaanderen/parcel-registry/commit/b037b1c6c6a2760dbe2f95a1a6a54b37b93b5b8f))
* return etag in detail and oslo V2 responses ([556774d](https://github.com/informatievlaanderen/parcel-registry/commit/556774d78d935892ca43d85b669d9999e44af9f1))
* sonarbug null check ([4ca134e](https://github.com/informatievlaanderen/parcel-registry/commit/4ca134e211db65ba9c97b119f8c4c66e7f2222a1))

## [4.8.4](https://github.com/informatievlaanderen/parcel-registry/compare/v4.8.3...v4.8.4) (2022-12-19)


### Bug Fixes

* detach address in detail projection ([d34deb2](https://github.com/informatievlaanderen/parcel-registry/commit/d34deb23fec2e3846d33b6f149224a4df3c2b825))

## [4.8.3](https://github.com/informatievlaanderen/parcel-registry/compare/v4.8.2...v4.8.3) (2022-12-16)


### Bug Fixes

* bump grar-common packages ([4f512ef](https://github.com/informatievlaanderen/parcel-registry/commit/4f512ef65f36e0011fb89232c0705e98e0e0c0ee))
* detail url should contain capakey instead of parcelid ([#520](https://github.com/informatievlaanderen/parcel-registry/issues/520)) ([e149da0](https://github.com/informatievlaanderen/parcel-registry/commit/e149da04631af705fcde496f361435046243de13))

## [4.8.2](https://github.com/informatievlaanderen/parcel-registry/compare/v4.8.1...v4.8.2) (2022-12-15)


### Bug Fixes

* register edit module in address consumer ([f7e6c29](https://github.com/informatievlaanderen/parcel-registry/commit/f7e6c2941090b1b1b0d503b6acb7a5a006c5e0e4))

## [4.8.1](https://github.com/informatievlaanderen/parcel-registry/compare/v4.8.0...v4.8.1) (2022-12-15)


### Performance Improvements

* use dbContextFactory for dbContext resolution ([4c6369c](https://github.com/informatievlaanderen/parcel-registry/commit/4c6369c15a87ece6042458b609dd6151434ec959))

# [4.8.0](https://github.com/informatievlaanderen/parcel-registry/compare/v4.7.4...v4.8.0) (2022-12-14)


### Features

* introduce idempotency in consumer ([7eb3954](https://github.com/informatievlaanderen/parcel-registry/commit/7eb39548cc3b309011e3bffd4c540c2ad408c62d))

## [4.7.4](https://github.com/informatievlaanderen/parcel-registry/compare/v4.7.3...v4.7.4) (2022-12-14)


### Bug Fixes

* documentation events ([33fc64d](https://github.com/informatievlaanderen/parcel-registry/commit/33fc64dda4a8bb08ff86abc776c8d0da4e8c539a))

## [4.7.3](https://github.com/informatievlaanderen/parcel-registry/compare/v4.7.2...v4.7.3) (2022-12-14)


### Bug Fixes

* address id documentation ([c34078c](https://github.com/informatievlaanderen/parcel-registry/commit/c34078c3272df8523c65e2f532082b740014a943))

## [4.7.2](https://github.com/informatievlaanderen/parcel-registry/compare/v4.7.1...v4.7.2) (2022-12-13)


### Bug Fixes

* add error logging for consumers ([6d2c590](https://github.com/informatievlaanderen/parcel-registry/commit/6d2c5900af28f0871e89d6ce3c31e132d7fe6c05))
* errorcodes & documentation ([8a9f66a](https://github.com/informatievlaanderen/parcel-registry/commit/8a9f66a76398e48c396dce8909758aafd351a406))

## [4.7.1](https://github.com/informatievlaanderen/parcel-registry/compare/v4.7.0...v4.7.1) (2022-12-12)

# [4.7.0](https://github.com/informatievlaanderen/parcel-registry/compare/v4.6.2...v4.7.0) (2022-12-09)


### Features

* add commandhandling projections for address ([d4fc844](https://github.com/informatievlaanderen/parcel-registry/commit/d4fc844e816e500e25feed8ceb333553e9015966))

## [4.6.2](https://github.com/informatievlaanderen/parcel-registry/compare/v4.6.1...v4.6.2) (2022-12-07)


### Bug Fixes

* documentation request examples ([0922b52](https://github.com/informatievlaanderen/parcel-registry/commit/0922b52a316a68a00439bab133970dc199fb70f2))

## [4.6.1](https://github.com/informatievlaanderen/parcel-registry/compare/v4.6.0...v4.6.1) (2022-12-06)


### Bug Fixes

* backoffice registration ([13566a2](https://github.com/informatievlaanderen/parcel-registry/commit/13566a2e1327374a1563dfb0d5e05c1f9c9d4e0b))
* remove comment for publish abstractions nuget ([80fdc39](https://github.com/informatievlaanderen/parcel-registry/commit/80fdc39f2863cac7a9d755062246d934207cc8fe))

# [4.6.0](https://github.com/informatievlaanderen/parcel-registry/compare/v4.5.2...v4.6.0) (2022-12-06)


### Features

* add addressId filter on legacy/oslo list ([881f5f4](https://github.com/informatievlaanderen/parcel-registry/commit/881f5f4c9bef2e17b30aefb817a3d438ef90e3bc))

## [4.5.2](https://github.com/informatievlaanderen/parcel-registry/compare/v4.5.1...v4.5.2) (2022-12-05)


### Bug Fixes

* ci release token ([24196df](https://github.com/informatievlaanderen/parcel-registry/commit/24196df06ba69bb7bb01ea2d2230540ae49622b9))
* correct ci checkout action ([bb7aa6e](https://github.com/informatievlaanderen/parcel-registry/commit/bb7aa6e527ee69d1be067da8352a62fb25fe37e8))
* extend syndication response ([673fde8](https://github.com/informatievlaanderen/parcel-registry/commit/673fde8a7859a0eeb987a6b985f8317aa259463d))
* remove old workflows ([c832549](https://github.com/informatievlaanderen/parcel-registry/commit/c832549100b71c53a52a768c567a8e307e7b3080))
* style to trigger release ([6142e05](https://github.com/informatievlaanderen/parcel-registry/commit/6142e05c852eca9e0e46e22824e573e2576e3fb6))

## [4.5.1](https://github.com/informatievlaanderen/parcel-registry/compare/v4.5.0...v4.5.1) (2022-12-05)


### Bug Fixes

* style to trigger build ([423c06e](https://github.com/informatievlaanderen/parcel-registry/commit/423c06e8116ac22b88e387d1efda30fc7b39a8b8))

# [4.5.0](https://github.com/informatievlaanderen/parcel-registry/compare/v4.4.5...v4.5.0) (2022-12-05)


### Features

* detach address ([764aff8](https://github.com/informatievlaanderen/parcel-registry/commit/764aff8e71ae804359787c4346f33c33adbe5fef))

## [4.4.5](https://github.com/informatievlaanderen/parcel-registry/compare/v4.4.4...v4.4.5) (2022-12-04)


### Bug Fixes

* remove debug docker images ([1410817](https://github.com/informatievlaanderen/parcel-registry/commit/14108173e2b34cb96e4f9ef86e8afb001ff68a63))

## [4.4.4](https://github.com/informatievlaanderen/parcel-registry/compare/v4.4.3...v4.4.4) (2022-12-04)


### Bug Fixes

* add docker images ([ffcd98e](https://github.com/informatievlaanderen/parcel-registry/commit/ffcd98e9b6305583a7cc888ded85edb4b0ef4ad7))

## [4.4.3](https://github.com/informatievlaanderen/parcel-registry/compare/v4.4.2...v4.4.3) (2022-12-03)


### Bug Fixes

* relax dependabot schedule ([9589705](https://github.com/informatievlaanderen/parcel-registry/commit/95897056ea1ed5734d75ea4c8c9bee1a657e692e))
* remove lambda ([a0914ab](https://github.com/informatievlaanderen/parcel-registry/commit/a0914aba8db3ccd42451e8c360457209260ac375))

## [4.4.2](https://github.com/informatievlaanderen/parcel-registry/compare/v4.4.1...v4.4.2) (2022-12-03)


### Bug Fixes

* make classes static ([2a0cbfb](https://github.com/informatievlaanderen/parcel-registry/commit/2a0cbfba2961fa085777fc19c58a5445e73fee60))

## [4.4.1](https://github.com/informatievlaanderen/parcel-registry/compare/v4.4.0...v4.4.1) (2022-12-03)


### Bug Fixes

* remove api-backoffice ([9aebc47](https://github.com/informatievlaanderen/parcel-registry/commit/9aebc4775e0ab34346e4f5d96556babfc719c3c4))

# [4.4.0](https://github.com/informatievlaanderen/parcel-registry/compare/v4.3.2...v4.4.0) (2022-12-02)


### Bug Fixes

* add backoffice-api to servicematrix in test & staging ([1b2f5fa](https://github.com/informatievlaanderen/parcel-registry/commit/1b2f5fac8095e62de7c376617c32bbaaafb07be3))
* change workflow ([6265f2e](https://github.com/informatievlaanderen/parcel-registry/commit/6265f2e96ce6af68a32d8f83d7e0dc26c79c758d))
* correct atlassian project ([15f627a](https://github.com/informatievlaanderen/parcel-registry/commit/15f627af65bfc9c89e3da835d59063afe3587ee7))
* correct release-new.yml for slack notification ([5fb48cd](https://github.com/informatievlaanderen/parcel-registry/commit/5fb48cdbd2a663d24ccb21748954313cc9381d1b))


### Features

* add attachAddress projections ([bc523f3](https://github.com/informatievlaanderen/parcel-registry/commit/bc523f35ecd7919e9b679074b01a3877d9c2b657))
* attach address ([#479](https://github.com/informatievlaanderen/parcel-registry/issues/479)) ([2b06af8](https://github.com/informatievlaanderen/parcel-registry/commit/2b06af890c3edd6c5ae40ee6ef0ba02146967822))

## [4.3.2](https://github.com/informatievlaanderen/parcel-registry/compare/v4.3.1...v4.3.2) (2022-11-29)


### Bug Fixes

* add SkipNotFoundAddress migrator toggle ([0bcbbe8](https://github.com/informatievlaanderen/parcel-registry/commit/0bcbbe85c9722ce96780c8663701dbed1f846b40))

## [4.3.1](https://github.com/informatievlaanderen/parcel-registry/compare/v4.3.0...v4.3.1) (2022-11-28)


### Bug Fixes

* deploy migrator ([79b18b5](https://github.com/informatievlaanderen/parcel-registry/commit/79b18b5ed887b7cd49252afaec99911438dfbcd5))

# [4.3.0](https://github.com/informatievlaanderen/parcel-registry/compare/v4.2.2...v4.3.0) (2022-11-28)


### Features

* implement v2 toggle on api's ([873739e](https://github.com/informatievlaanderen/parcel-registry/commit/873739e0cde17add17d6d9caffae5805f903e2cc))

## [4.2.2](https://github.com/informatievlaanderen/parcel-registry/compare/v4.2.1...v4.2.2) (2022-11-25)


### Bug Fixes

* address consumer ([#477](https://github.com/informatievlaanderen/parcel-registry/issues/477)) ([9682cb5](https://github.com/informatievlaanderen/parcel-registry/commit/9682cb5efff650b88a8244d5b69a098ce3408ab4))
* extend removed check GRAR-3581 ([32f96af](https://github.com/informatievlaanderen/parcel-registry/commit/32f96af1bf0c9220a95fcd09c4812081837edf6a))

## [4.2.1](https://github.com/informatievlaanderen/parcel-registry/compare/v4.2.0...v4.2.1) (2022-11-25)


### Bug Fixes

* remove migrator from deploy to stg ([21362db](https://github.com/informatievlaanderen/parcel-registry/commit/21362dbc0f9a1df8af9538858c4360b688e9deb4))

# [4.2.0](https://github.com/informatievlaanderen/parcel-registry/compare/v4.1.9...v4.2.0) (2022-11-24)


### Features

* add ability to fix GRAR3581 invalid parcel state ([#471](https://github.com/informatievlaanderen/parcel-registry/issues/471)) ([0965e98](https://github.com/informatievlaanderen/parcel-registry/commit/0965e98f306d2d2991e2e54e5748099288d25d0c))
* add address consumer ([496b80b](https://github.com/informatievlaanderen/parcel-registry/commit/496b80b221eb6ff7b1dbb3860acedfe577e53c58))
* add backoffice proj ([5edde63](https://github.com/informatievlaanderen/parcel-registry/commit/5edde636e8424a015432759d39e5adf5208f7854))
* add migrator ([0610725](https://github.com/informatievlaanderen/parcel-registry/commit/061072517fe54877f056a275969a561fefc755ac))
* add V2 projections ([06c3baf](https://github.com/informatievlaanderen/parcel-registry/commit/06c3baf197da815dd13b48210f1f826961a6ec11))
* mark as migrated ([6a44512](https://github.com/informatievlaanderen/parcel-registry/commit/6a4451284a7c66a6b4dd3eef908d0fa92c5d0f6b))
* new aggregate snapshotting + migration event and command ([340593e](https://github.com/informatievlaanderen/parcel-registry/commit/340593e8be7f0757086598a4b920789e6cac6aa4))

## [4.1.9](https://github.com/informatievlaanderen/parcel-registry/compare/v4.1.8...v4.1.9) (2022-11-03)


### Bug Fixes

* add nuget to dependabot ([4faf8b3](https://github.com/informatievlaanderen/parcel-registry/commit/4faf8b37bc1a500f398977c14ae3600330dd2205))
* enable pr's & coverage ([#446](https://github.com/informatievlaanderen/parcel-registry/issues/446)) ([1d2d989](https://github.com/informatievlaanderen/parcel-registry/commit/1d2d989a20e7a580c49dc9eb051b5475ce023ab6))
* fix empty methods ([11c2ce0](https://github.com/informatievlaanderen/parcel-registry/commit/11c2ce0c2202948b303f360387bd132a9a7735b8))
* update ci & test branch protection ([93a5b3b](https://github.com/informatievlaanderen/parcel-registry/commit/93a5b3b576e672777844309b06cc6436675e4053))
* use VBR_SONAR_TOKEN ([cd594f0](https://github.com/informatievlaanderen/parcel-registry/commit/cd594f02821e0e8c9bac3809afa4dfd5150457e9))

## [4.1.8](https://github.com/informatievlaanderen/parcel-registry/compare/v4.1.7...v4.1.8) (2022-10-05)


### Bug Fixes

* xml serialization syndication ([47beaf5](https://github.com/informatievlaanderen/parcel-registry/commit/47beaf5c2c3ebaa1a811f431ba62e92d3327d4fb))

## [4.1.7](https://github.com/informatievlaanderen/parcel-registry/compare/v4.1.6...v4.1.7) (2022-09-06)


### Bug Fixes

* change workflow ([15847f6](https://github.com/informatievlaanderen/parcel-registry/commit/15847f67797a475c03a061131b8073ebdc81ca91))

## [4.1.6](https://github.com/informatievlaanderen/parcel-registry/compare/v4.1.5...v4.1.6) (2022-09-05)


### Bug Fixes

* fix format strings ([83940a4](https://github.com/informatievlaanderen/parcel-registry/commit/83940a40d7879c042a1574c3d99d467c4c5cbacb))

## [4.1.5](https://github.com/informatievlaanderen/parcel-registry/compare/v4.1.4...v4.1.5) (2022-08-26)


### Bug Fixes

* correct type to find address syndication ([3e183e2](https://github.com/informatievlaanderen/parcel-registry/commit/3e183e2191767fcbefc90068d6b6b7d88f0be2ed))

## [4.1.4](https://github.com/informatievlaanderen/parcel-registry/compare/v4.1.3...v4.1.4) (2022-08-26)


### Bug Fixes

* style to trigger build ([6203c13](https://github.com/informatievlaanderen/parcel-registry/commit/6203c1357b16f5ca8e2a0b5b11ecbff7e70beb28))

## [4.1.3](https://github.com/informatievlaanderen/parcel-registry/compare/v4.1.2...v4.1.3) (2022-08-26)


### Bug Fixes

* bump common packages ([f69e845](https://github.com/informatievlaanderen/parcel-registry/commit/f69e8457027c79fbf18d5bc9bd0f3e1d732f50b8))

## [4.1.2](https://github.com/informatievlaanderen/parcel-registry/compare/v4.1.1...v4.1.2) (2022-08-24)


### Bug Fixes

* bump command-handling ([27f881c](https://github.com/informatievlaanderen/parcel-registry/commit/27f881c78cae1b0d820f8c92af382891cc2bc888))

## [4.1.1](https://github.com/informatievlaanderen/parcel-registry/compare/v4.1.0...v4.1.1) (2022-08-24)


### Bug Fixes

* disable snapshotting ([ac4c3a9](https://github.com/informatievlaanderen/parcel-registry/commit/ac4c3a92e0445c929252b6de99ac133b73829a11))

# [4.1.0](https://github.com/informatievlaanderen/parcel-registry/compare/v4.0.9...v4.1.0) (2022-08-22)


### Features

* bump deps ([0b0ff7c](https://github.com/informatievlaanderen/parcel-registry/commit/0b0ff7c404ef5bf0939a8789a875176e57a71f37))

## [4.0.9](https://github.com/informatievlaanderen/parcel-registry/compare/v4.0.8...v4.0.9) (2022-08-16)


### Bug Fixes

* correct address syndiction streetnameid type ([f966a18](https://github.com/informatievlaanderen/parcel-registry/commit/f966a1875f97a461e6cd8a81a13d7bc1184c189f))

## [4.0.8](https://github.com/informatievlaanderen/parcel-registry/compare/v4.0.7...v4.0.8) (2022-08-16)


### Bug Fixes

* correct syndication event dto ([da29a3a](https://github.com/informatievlaanderen/parcel-registry/commit/da29a3aeb05d3073252db733790d75125c9d9f6d))

## [4.0.7](https://github.com/informatievlaanderen/parcel-registry/compare/v4.0.6...v4.0.7) (2022-08-10)


### Bug Fixes

* revert "fix: correct format strings" ([6fa7632](https://github.com/informatievlaanderen/parcel-registry/commit/6fa7632cc1a057f2c4cce0f4a365f1682834bdbd))

## [4.0.6](https://github.com/informatievlaanderen/parcel-registry/compare/v4.0.5...v4.0.6) (2022-08-01)


### Bug Fixes

* conform to Serializable ([c7fbd1b](https://github.com/informatievlaanderen/parcel-registry/commit/c7fbd1b91b48388bdfbda65be167f3a49f1b6d82))
* correct format strings ([32507ce](https://github.com/informatievlaanderen/parcel-registry/commit/32507ce0ac37d827cfa876d3080e6f26cfda122c))
* correct parameter names ([cda38b9](https://github.com/informatievlaanderen/parcel-registry/commit/cda38b9825b180f897eb1352ceffe2c99eebb32e))
* don't throw general exceptions ([b6a52fc](https://github.com/informatievlaanderen/parcel-registry/commit/b6a52fce2431b07430258ceaffc43dd419c0bedf))
* empty methods ([4a36dfd](https://github.com/informatievlaanderen/parcel-registry/commit/4a36dfda126e33caa733b78e000a06e4e60ef7e3))
* nested ternary clauses ([ec40c02](https://github.com/informatievlaanderen/parcel-registry/commit/ec40c026ba993413d214f5f5fb8b4a3805a08322))
* utility classes static or protected ctor ([d8682bb](https://github.com/informatievlaanderen/parcel-registry/commit/d8682bb538f9b054186bc41c3796a49eeefcaf5e))
* visibility of fields ([e5a08b5](https://github.com/informatievlaanderen/parcel-registry/commit/e5a08b50dcac51c9899da9d0e2c7787cd4125219))

## [4.0.5](https://github.com/informatievlaanderen/parcel-registry/compare/v4.0.4...v4.0.5) (2022-06-30)


### Bug Fixes

* add LABEL to Dockerfile (for easier DataDog filtering) ([32317de](https://github.com/informatievlaanderen/parcel-registry/commit/32317deca6898eed1d62408492e1ffd6da01e1bc))

## [4.0.4](https://github.com/informatievlaanderen/parcel-registry/compare/v4.0.3...v4.0.4) (2022-04-29)


### Bug Fixes

* run sonar end when release version != none ([e97b23a](https://github.com/informatievlaanderen/parcel-registry/commit/e97b23a7a3579645848dc4905e00527e739bcf07))

## [4.0.3](https://github.com/informatievlaanderen/parcel-registry/compare/v4.0.2...v4.0.3) (2022-04-27)


### Bug Fixes

* redirect sonar to /dev/null ([db40645](https://github.com/informatievlaanderen/parcel-registry/commit/db4064552d4e456560300b0bd2b7d7787ebb29a0))

## [4.0.2](https://github.com/informatievlaanderen/parcel-registry/compare/v4.0.1...v4.0.2) (2022-04-04)


### Bug Fixes

* set oslo context type to string GAWR-2931 ([0834085](https://github.com/informatievlaanderen/parcel-registry/commit/0834085764a045058c3a22dc9a1422ddb3b4d3ec))

## [4.0.1](https://github.com/informatievlaanderen/parcel-registry/compare/v4.0.0...v4.0.1) (2022-03-30)


### Bug Fixes

* update CODEOWNERS to trigger build ([0204c07](https://github.com/informatievlaanderen/parcel-registry/commit/0204c07dd84923fed0aff1ea61bce2b24e6a9ec3))

# [4.0.0](https://github.com/informatievlaanderen/parcel-registry/compare/v3.29.0...v4.0.0) (2022-03-30)


### Features

* move to dotnet 6.0.3 ([3b00d77](https://github.com/informatievlaanderen/parcel-registry/commit/3b00d772da2babb6278ad37b5ebe87c9378d3adc))


### BREAKING CHANGES

* move to dotnet 6.0.3

# [3.29.0](https://github.com/informatievlaanderen/parcel-registry/compare/v3.28.6...v3.29.0) (2022-02-25)


### Features

* update api to 17.0.0 ([810d345](https://github.com/informatievlaanderen/parcel-registry/commit/810d3459713302107a107f594d65fbfd37895341))

## [3.28.6](https://github.com/informatievlaanderen/parcel-registry/compare/v3.28.5...v3.28.6) (2022-02-10)


### Bug Fixes

* update Api dependency to fix exception handler ([eac7350](https://github.com/informatievlaanderen/parcel-registry/commit/eac73505a8d28517598fa838794e06340717e308))

## [3.28.5](https://github.com/informatievlaanderen/parcel-registry/compare/v3.28.4...v3.28.5) (2022-01-21)


### Bug Fixes

* correctly resume projections async ([a3333a2](https://github.com/informatievlaanderen/parcel-registry/commit/a3333a277b206fdde2e6d88f83c5583bf5934b7b))

## [3.28.4](https://github.com/informatievlaanderen/parcel-registry/compare/v3.28.3...v3.28.4) (2022-01-17)

## [3.28.3](https://github.com/informatievlaanderen/parcel-registry/compare/v3.28.2...v3.28.3) (2021-12-21)


### Bug Fixes

* gawr-2496 & gwar-2499 api docs oslo eindpoints ([5052a6c](https://github.com/informatievlaanderen/parcel-registry/commit/5052a6ca632cdf48767266e2f41c57a93eb33233))

## [3.28.2](https://github.com/informatievlaanderen/parcel-registry/compare/v3.28.1...v3.28.2) (2021-12-17)


### Bug Fixes

* use async startup of projections to fix hanging migrations ([a2863d0](https://github.com/informatievlaanderen/parcel-registry/commit/a2863d060da0518601471d14e2edece497f6e6c5))

## [3.28.1](https://github.com/informatievlaanderen/parcel-registry/compare/v3.28.0...v3.28.1) (2021-12-13)


### Bug Fixes

* include jsonld accepttyp in lastchangedlistprojections ([1d342de](https://github.com/informatievlaanderen/parcel-registry/commit/1d342deadcfed91690af7f2e31bc8c6034762fc2))

# [3.28.0](https://github.com/informatievlaanderen/parcel-registry/compare/v3.27.2...v3.28.0) (2021-12-08)


### Features

* add projection handler ([b0b654e](https://github.com/informatievlaanderen/parcel-registry/commit/b0b654e6e4ec6e962c404af882d805e6abb70bfb))

## [3.27.2](https://github.com/informatievlaanderen/parcel-registry/compare/v3.27.1...v3.27.2) (2021-12-08)


### Bug Fixes

* github workflow ([05ae1e1](https://github.com/informatievlaanderen/parcel-registry/commit/05ae1e14999c8944278459d779f5f99dd9d91400))

## [3.27.1](https://github.com/informatievlaanderen/parcel-registry/compare/v3.27.0...v3.27.1) (2021-12-08)


### Bug Fixes

* remove event db connection, remove unused classes, unused methods and modify launchsettings ([ab6b24b](https://github.com/informatievlaanderen/parcel-registry/commit/ab6b24b9257cfc770cba4635e3b13c575d4dead7))

# [3.27.0](https://github.com/informatievlaanderen/parcel-registry/compare/v3.26.2...v3.27.0) (2021-12-08)


### Features

* add parcel v2 oslo endpoints ([044ccb5](https://github.com/informatievlaanderen/parcel-registry/commit/044ccb5ac6283a3d7aac7f2f63ad238be09ac0dd))

## [3.26.2](https://github.com/informatievlaanderen/parcel-registry/compare/v3.26.1...v3.26.2) (2021-11-17)


### Bug Fixes

* when address is already added, do not add again in projection ([4859c65](https://github.com/informatievlaanderen/parcel-registry/commit/4859c658640f5c299a3c2778a2e267fc9cd7a0bb))

## [3.26.1](https://github.com/informatievlaanderen/parcel-registry/compare/v3.26.0...v3.26.1) (2021-10-27)


### Bug Fixes

* remove default accesskey/secret ([fb17501](https://github.com/informatievlaanderen/parcel-registry/commit/fb17501aa520dc3604afb2f5609c3495bedfbe30))

# [3.26.0](https://github.com/informatievlaanderen/parcel-registry/compare/v3.25.7...v3.26.0) (2021-10-25)


### Features

* gawr 2202 paket bump ([46ba639](https://github.com/informatievlaanderen/parcel-registry/commit/46ba639def46f8b086b01036e55329513b6c8e53))

## [3.25.7](https://github.com/informatievlaanderen/parcel-registry/compare/v3.25.6...v3.25.7) (2021-10-21)


### Bug Fixes

* gawr-2202 api documentation changes ([3982180](https://github.com/informatievlaanderen/parcel-registry/commit/39821804dcf48e9a623fe37db22a7909bd208147))

## [3.25.6](https://github.com/informatievlaanderen/parcel-registry/compare/v3.25.5...v3.25.6) (2021-10-06)


### Bug Fixes

* gawr-615 add offset+2 to version id ([2da917e](https://github.com/informatievlaanderen/parcel-registry/commit/2da917ee9312334ff69b5a094a8f22cf87670447))

## [3.25.5](https://github.com/informatievlaanderen/parcel-registry/compare/v3.25.4...v3.25.5) (2021-10-06)


### Bug Fixes

* add Test to ECR ([1c6b47a](https://github.com/informatievlaanderen/parcel-registry/commit/1c6b47a2e12f6176d1a87b6662b638b3ab7b824e))

## [3.25.4](https://github.com/informatievlaanderen/parcel-registry/compare/v3.25.3...v3.25.4) (2021-10-01)


### Bug Fixes

* update-package ([ea68e46](https://github.com/informatievlaanderen/parcel-registry/commit/ea68e46111f7bc83557aae46a491ac4240e3df06))

## [3.25.3](https://github.com/informatievlaanderen/parcel-registry/compare/v3.25.2...v3.25.3) (2021-09-22)


### Bug Fixes

* gawr-611 fix exception detail ([d9b6500](https://github.com/informatievlaanderen/parcel-registry/commit/d9b650050a98dec8d55c5454993fcf6f4849971f))

## [3.25.2](https://github.com/informatievlaanderen/parcel-registry/compare/v3.25.1...v3.25.2) (2021-09-20)


### Bug Fixes

* update package ([23249cc](https://github.com/informatievlaanderen/parcel-registry/commit/23249ccde0ea311bf0b029a290356e136d74e58b))

## [3.25.1](https://github.com/informatievlaanderen/parcel-registry/compare/v3.25.0...v3.25.1) (2021-08-26)


### Bug Fixes

* update grar-common dependencies GRAR-2060 ([8c74a2f](https://github.com/informatievlaanderen/parcel-registry/commit/8c74a2ffb387689c34a9b27387ac1ea161cf3006))

# [3.25.0](https://github.com/informatievlaanderen/parcel-registry/compare/v3.24.6...v3.25.0) (2021-08-26)


### Features

* add metadata file with latest event id to parcel extract GRAR-2060 ([b279096](https://github.com/informatievlaanderen/parcel-registry/commit/b279096ad83c63bb80e4088cf4f54f471c12a5c2))

## [3.24.6](https://github.com/informatievlaanderen/parcel-registry/compare/v3.24.5...v3.24.6) (2021-06-25)


### Bug Fixes

* update aws DistributedMutex package ([a7b8af9](https://github.com/informatievlaanderen/parcel-registry/commit/a7b8af97f8a07e65a83b089f7cd9a77d0daa4134))

## [3.24.5](https://github.com/informatievlaanderen/parcel-registry/compare/v3.24.4...v3.24.5) (2021-06-25)


### Bug Fixes

* added unique constraint to the persistentlocalid ([7a78692](https://github.com/informatievlaanderen/parcel-registry/commit/7a786920ea3025a803e6afc09838af95f7b842f6))

## [3.24.4](https://github.com/informatievlaanderen/parcel-registry/compare/v3.24.3...v3.24.4) (2021-06-17)


### Bug Fixes

*  update nuget pakage ([bef1ae3](https://github.com/informatievlaanderen/parcel-registry/commit/bef1ae34a7067e18ab5f0a977986d1562c29717a))

## [3.24.3](https://github.com/informatievlaanderen/parcel-registry/compare/v3.24.2...v3.24.3) (2021-05-31)


### Bug Fixes

* update api ([cdbcd6f](https://github.com/informatievlaanderen/parcel-registry/commit/cdbcd6f406f9ac07a2e176ed005a1c180461beb9))

## [3.24.2](https://github.com/informatievlaanderen/parcel-registry/compare/v3.24.1...v3.24.2) (2021-05-31)


### Bug Fixes

* update pipeline & api ([a69eb92](https://github.com/informatievlaanderen/parcel-registry/commit/a69eb92b6800b9e9560546b167ef7db39be39d21))

## [3.24.1](https://github.com/informatievlaanderen/parcel-registry/compare/v3.24.0...v3.24.1) (2021-05-29)


### Bug Fixes

* move to 5.0.6 ([6505a6e](https://github.com/informatievlaanderen/parcel-registry/commit/6505a6e16529caf6417530f0b5c69fe27c21cb12))

# [3.24.0](https://github.com/informatievlaanderen/parcel-registry/compare/v3.23.3...v3.24.0) (2021-05-04)


### Features

* bump packages ([350cedb](https://github.com/informatievlaanderen/parcel-registry/commit/350cedb0995f9695264051b1eca570dd1f274d23))

## [3.23.3](https://github.com/informatievlaanderen/parcel-registry/compare/v3.23.2...v3.23.3) (2021-04-26)


### Bug Fixes

* rename cache status endpoint in projector ([0f91af2](https://github.com/informatievlaanderen/parcel-registry/commit/0f91af202c0a92e0ceec856f1f0f1d312b27b916))

## [3.23.2](https://github.com/informatievlaanderen/parcel-registry/compare/v3.23.1...v3.23.2) (2021-04-26)


### Bug Fixes

* projections can handle snapshot events ([297f6bd](https://github.com/informatievlaanderen/parcel-registry/commit/297f6bdc77aa3b986205904ea2592f493fc7435d))

## [3.23.1](https://github.com/informatievlaanderen/parcel-registry/compare/v3.23.0...v3.23.1) (2021-04-23)


### Bug Fixes

* bump snapshot packages ([3793db7](https://github.com/informatievlaanderen/parcel-registry/commit/3793db72801db796f28346ca21c44aafb60ad62f))

# [3.23.0](https://github.com/informatievlaanderen/parcel-registry/compare/v3.22.8...v3.23.0) (2021-04-22)


### Features

* bump grar-common packages ([016634c](https://github.com/informatievlaanderen/parcel-registry/commit/016634cae4a95843dc760261c22b8670d4e1dce7))

## [3.22.8](https://github.com/informatievlaanderen/parcel-registry/compare/v3.22.7...v3.22.8) (2021-04-19)


### Bug Fixes

* removed xunit config to run in serial and use collections ([80d5667](https://github.com/informatievlaanderen/parcel-registry/commit/80d5667d56d2d4842a97912fd6d7caacc10eb7ea))

## [3.22.7](https://github.com/informatievlaanderen/parcel-registry/compare/v3.22.6...v3.22.7) (2021-04-19)


### Bug Fixes

* revert "fix: rewrote tests to try to fix build" ([cce1d0c](https://github.com/informatievlaanderen/parcel-registry/commit/cce1d0cf3b34ef86c1b6365abf0c35a33c0221a8))
* use default build script ([f95f31b](https://github.com/informatievlaanderen/parcel-registry/commit/f95f31be679ac8e612907a408175a1dd468ce21c))

## [3.22.6](https://github.com/informatievlaanderen/parcel-registry/compare/v3.22.5...v3.22.6) (2021-04-19)


### Bug Fixes

* add config to run tests in serial ([5d9d2cf](https://github.com/informatievlaanderen/parcel-registry/commit/5d9d2cf19313f060364b8db1a6feb4a9d11b9171))
* build add verbosity logging ([6acca23](https://github.com/informatievlaanderen/parcel-registry/commit/6acca23f50ba312ccad6092a7db487bfdd454997))
* build only output test detailed ([93f214b](https://github.com/informatievlaanderen/parcel-registry/commit/93f214b73df2e95eb79d2f467a1a789f08b39f8e))
* build to 5.0.102 ([4ce60f0](https://github.com/informatievlaanderen/parcel-registry/commit/4ce60f0bc66eb8da38206bb0cfcd4a48c1ef41e4))
* bump dotnet sdk to 5.0.104 and clr 5.0.4 ([d495672](https://github.com/informatievlaanderen/parcel-registry/commit/d495672ebcc04bcbccbd799d80b27c73f78c2a6d))
* bump Test SDK ([8b10114](https://github.com/informatievlaanderen/parcel-registry/commit/8b10114c0f7955eae5c95472f78f6b99a455d986))
* fix build enable diagnostic logging for tests ([a026c88](https://github.com/informatievlaanderen/parcel-registry/commit/a026c8810d767e6cdbd64c6fa982c46bde9045b7))
* rewrote tests to try to fix build ([1f4385d](https://github.com/informatievlaanderen/parcel-registry/commit/1f4385de70d105e3079dcbd4853b1adaf29a634e))
* try to fix build with knowing working test ([a9a108d](https://github.com/informatievlaanderen/parcel-registry/commit/a9a108d72cc911a0e3d9e69df03c9bd0c3a6c3ce))

## [3.22.5](https://github.com/informatievlaanderen/parcel-registry/compare/v3.22.4...v3.22.5) (2021-04-16)


### Bug Fixes

* enabled 3 more snapshot tests ([0c66964](https://github.com/informatievlaanderen/parcel-registry/commit/0c6696474efa23c088796b1a727f5463eb3eaf68))

## [3.22.4](https://github.com/informatievlaanderen/parcel-registry/compare/v3.22.3...v3.22.4) (2021-04-16)


### Bug Fixes

* enabled one more test to try build ([44dc689](https://github.com/informatievlaanderen/parcel-registry/commit/44dc689c0b7ac57c7e1ef0bcb339836f014db1f8))

## [3.22.3](https://github.com/informatievlaanderen/parcel-registry/compare/v3.22.2...v3.22.3) (2021-04-16)


### Bug Fixes

* disabled 2 out of 3 snapshottests to test build ([b91d2d6](https://github.com/informatievlaanderen/parcel-registry/commit/b91d2d66631c081b12eff750b3a3a40510deb956))
* enabled 3 basedonsnapshot tests to test build ([dd1d945](https://github.com/informatievlaanderen/parcel-registry/commit/dd1d9457823dd24561f2dfd6a36551af50b842fb))

## [3.22.2](https://github.com/informatievlaanderen/parcel-registry/compare/v3.22.1...v3.22.2) (2021-04-16)


### Bug Fixes

* disable test to see if build works ([358225e](https://github.com/informatievlaanderen/parcel-registry/commit/358225ed4bedadc4887cbffa7ba643470eeb19bd))
* disabled basedonsnapshot tests to see if build works ([6c9058d](https://github.com/informatievlaanderen/parcel-registry/commit/6c9058ddb7db1c2fc2a3dc69819545fee8b3b085))

## [3.22.1](https://github.com/informatievlaanderen/parcel-registry/compare/v3.22.0...v3.22.1) (2021-04-15)


### Bug Fixes

* set parcelid when retrieving snapshot + add snapshot tests ([4e0ac47](https://github.com/informatievlaanderen/parcel-registry/commit/4e0ac47e57ada951a8f375dd644595a46ea2e1ba))

# [3.22.0](https://github.com/informatievlaanderen/parcel-registry/compare/v3.21.0...v3.22.0) (2021-04-12)


### Features

* add snapshotting ([8b46ed7](https://github.com/informatievlaanderen/parcel-registry/commit/8b46ed7d9327fe9e605b224b981032634f61aae4))

# [3.21.0](https://github.com/informatievlaanderen/parcel-registry/compare/v3.20.1...v3.21.0) (2021-04-01)


### Bug Fixes

* update docs projections ([9c9fad9](https://github.com/informatievlaanderen/parcel-registry/commit/9c9fad912cdfdb23be26294952e78981f28e8be4))


### Features

* bump projector & projection-handling ([da5dc0b](https://github.com/informatievlaanderen/parcel-registry/commit/da5dc0b6eb79d20db98a431522d0cb199dcfa0bc))

## [3.20.1](https://github.com/informatievlaanderen/parcel-registry/compare/v3.20.0...v3.20.1) (2021-03-22)


### Bug Fixes

* remove ridingwolf, collaboration ended ([ab96c1c](https://github.com/informatievlaanderen/parcel-registry/commit/ab96c1ccd0abca1ecd9b9088e705e1f5e79c03ae))

# [3.20.0](https://github.com/informatievlaanderen/parcel-registry/compare/v3.19.3...v3.20.0) (2021-03-11)


### Bug Fixes

* update projector dependency GRAR-1876 ([1084ce9](https://github.com/informatievlaanderen/parcel-registry/commit/1084ce9236fc32228a66aacbd5a08fd861301609))


### Features

* add projection attributes GRAR-1876 ([fe0e976](https://github.com/informatievlaanderen/parcel-registry/commit/fe0e976fad5b2225b897717a08d23e165f932aa6))

## [3.19.3](https://github.com/informatievlaanderen/parcel-registry/compare/v3.19.2...v3.19.3) (2021-02-15)


### Bug Fixes

* register problem details helper for projector GRAR-1814 ([d7fe02e](https://github.com/informatievlaanderen/parcel-registry/commit/d7fe02ebec1e5e136ba08b7472f88495bdc7718b))

## [3.19.2](https://github.com/informatievlaanderen/parcel-registry/compare/v3.19.1...v3.19.2) (2021-02-11)


### Bug Fixes

* update api with use of problemdetailshelper GRAR-1814 ([e7277ec](https://github.com/informatievlaanderen/parcel-registry/commit/e7277ecc19e2b3499845aeb1e2fbfc6f818bfa12))

## [3.19.1](https://github.com/informatievlaanderen/parcel-registry/compare/v3.19.0...v3.19.1) (2021-02-02)


### Bug Fixes

* move to 5.0.2 ([6e9144e](https://github.com/informatievlaanderen/parcel-registry/commit/6e9144e407a48fdf00e4539eba60ecc8e2698264))

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
