#!/bin/bash

set -x
set -m

/entrypoint.sh couchbase-server &

sleep 15

# Setup index and memory quota
curl -v -X POST http://127.0.0.1:8091/pools/default -d memoryQuota=300 -d indexMemoryQuota=300

# Setup services
curl -v http://127.0.0.1:8091/node/controller/setupServices -d services=kv%2Cn1ql%2Cindex

# Setup credentials
curl -v http://127.0.0.1:8091/settings/web -d port=8091 -d username=Administrator -d password=123456

# Setup Memory Optimized Indexes
curl -i -u Administrator:123456 -X POST http://127.0.0.1:8091/settings/indexes -d 'storageMode=memory_optimized'

# Load test couchbase bucket
couchbase-cli bucket-create -c 127.0.0.1:8091 -u Administrator -p 123456 --bucket=ProductContext --bucket-type=couchbase --bucket-port=11222 --bucket-ramsize=200 --bucket-replica=1

echo "Type: $TYPE"

if [ "$TYPE" = "WORKER" ]; then
  echo "Sleeping ..."
  sleep 15

  #IP=`hostname -s`
  IP=`hostname -I | cut -d ' ' -f1`
  echo "IP: " $IP

  echo "Auto Rebalance: $AUTO_REBALANCE"
  if [ "$AUTO_REBALANCE" = "true" ]; then
    couchbase-cli rebalance --cluster=$COUCHBASE_MASTER:8091 --user=Administrator --password=123456 --server-add=$IP --server-add-username=Administrator --server-add-password=123456
  else
    couchbase-cli server-add --cluster=$COUCHBASE_MASTER:8091 --user=Administrator --password=123456 --server-add=$IP --server-add-username=Administrator --server-add-password=123456
  fi;
fi;

fg 1