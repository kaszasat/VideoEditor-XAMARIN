import sys
import logging

#LOG_FORMAT = '%(levelname)s %(asctime)s %(message)s %(funcName)s %(lineno)d %(processName)s %(thread)d'

logging.basicConfig(
    filename='server.log',
    filemode='a',
    #format=LOG_FORMAT,
    level=logging.DEBUG
)
LOG = logging.getLogger()
LOG.addHandler(logging.StreamHandler(stream=sys.stdout))
