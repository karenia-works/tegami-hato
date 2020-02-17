FROM postgres:12-alpine
RUN sed -i 's/dl-cdn.alpinelinux.org/mirrors.tuna.tsinghua.edu.cn/g' /etc/apk/repositories 
RUN apk add --no-cache --virtual .fetch-deps \
        ca-certificates \
        openssl \
        tar git make postgresql-libs postgresql-dev 
WORKDIR /tmp/jieba
RUN git clone https://github.com/jaiminpan/pg_jieba.git --recursive --depth=5 --shallow-submodules
RUN apk add --no-cache --virtual .build-deps \
        autoconf \
        automake \
        g++ \
        json-c-dev \
        libtool \
        libxml2-dev \
        make cmake \
        perl 
WORKDIR /tmp/jieba/pg_jieba
RUN mkdir build && cd build && cmake ..
WORKDIR /tmp/jieba/pg_jieba/build
RUN make 
RUN make install
RUN apk del .fetch-deps .build-deps

