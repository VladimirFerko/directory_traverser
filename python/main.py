import os
import json

from current_directory import CurrentDirectory
import utils as ut



if __name__ == '__main__':
    instance = ut.create_or_load_instance()

    ut.get_action(instance)
