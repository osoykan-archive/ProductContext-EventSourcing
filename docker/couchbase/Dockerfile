FROM couchbase:latest

COPY configure.sh /opt/couchbase
COPY by_productId.ddoc /opt/couchbase

RUN chmod +x /opt/couchbase/configure.sh

CMD ["/opt/couchbase/configure.sh"]
