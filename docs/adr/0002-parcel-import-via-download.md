# 2. Parcel Import via Download Toepassingen

Date: 2023-05-21

## Status

Accepted

## Context

We need to add/update the parcels.
GRB is the source of that information

## Decision

We will download the parcels from GRB via "Download Toepassingen" and process them.
The option of GRB sending the parcels to us has been considered, but would take also development time from them.

## Consequences

We will need to create a ACM/IDM Client to authenticate with Download Toepassingen.
We will need to create a `Importer.Grb` to process the files and parcels.
