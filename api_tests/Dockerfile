FROM python:3.7-alpine

RUN mkdir /tests && \
  pip install nose requests

COPY . /tests

WORKDIR /tests
CMD ["nosetests", "--with-xunit", "--xunit-file", "/results/api-test-results.xml"]
