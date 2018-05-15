#!/bin/bash

set -x
set -m

/entrypoint.sh couchbase-server &

while [[ "$(curl -s -o /dev/null -w ''%{http_code}'' http://127.0.0.1:8091/pools/default)" != "404" ]]; do sleep 1; done

# Setup index and memory quota
curl -v -X POST http://127.0.0.1:8091/pools/default -d memoryQuota=300 -d indexMemoryQuota=300

# Setup services
curl -v http://127.0.0.1:8091/node/controller/setupServices -d services=kv%2Cn1ql%2Cindex

# Setup credentials
curl -v http://127.0.0.1:8091/settings/web -d port=8091 -d username=Administrator -d password=password

# Setup Memory Optimized Indexes
curl -i -u Administrator:password -X POST http://127.0.0.1:8091/settings/indexes -d 'storageMode=memory_optimized'

# Sleep 3 seconds
sleep 5

# Create ProductContext bucket
curl -X POST -u Administrator:password -d 'name=ProductContext' -d 'ramQuotaMB=20'0 -d 'authType=none' -d 'replicaNumber=0' -d 'proxyPort=11216' http://127.0.0.1:8091/pools/default/buckets
#curl -X PUT http://localhost:8091/settings/rbac/users/local/idxmanage -d "name=ProductContext&roles=query_manage_index[bucket1]&password=password" -u Administrator:password
#curl -X POST -v -u Administrator:password http://localhost:8093/query/service -d statement=CREATE%20PRIMARY%20INDEX%20primary_index%20ON%20ProductContext:ProductContext%20USING%20GSI


while [[ "$(curl -u Administrator:password -s -o /dev/null -w ''%{http_code}'' http://127.0.0.1:8091/pools/default/buckets/ProductContext)" != "200" ]]; do sleep 1; done

sleep 10

# Create by_productId view on ProductContext bucket.
curl -X POST -v -u Administrator:password http://127.0.0.1:8093/query/service -d 'statement=CREATE PRIMARY INDEX `Product_Index` ON `ProductContext` USING GSI WITH {"defer_build":false};'
curl -X POST -v -u Administrator:password http://127.0.0.1:8093/query/service -d 'statement=BUILD INDEX ON `ProductContext`(`Product_Index`) USING GSI;'
cp /opt/couchbase/by_productId.ddoc .
curl -X PUT -H 'Content-Type: application/json' http://Administrator:password@127.0.0.1:8092/ProductContext/_design/dev_by_productId	-d @by_productId.ddoc
curl -X PUT -H 'Content-Type: application/json' http://Administrator:password@127.0.0.1:8092/ProductContext/_design/by_productId	-d @by_productId.ddoc



fg 1