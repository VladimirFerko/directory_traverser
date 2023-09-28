import os
import json

from current_directory import CurrentDirectory

def get_directory_path():
    path = input('Please insert here path of desired directory: ')

    while not os.path.isdir(path):
        path = input('Please insert here path of desired directory: ')
    
    return path

def create_or_load_instance():
    choice = input("Do you want to create a new instance (enter 'create') or load from a JSON file (enter 'load')? ").strip().lower()

    while 1:
        if choice == 'create':

            path = get_directory_path()
            filename = input("Enter the JSON output filename: ")
            
            instance = CurrentDirectory(path, filename)
            return instance
        elif choice == 'load':
            
            while 1:
                json_filename = input("Enter the JSON filename or path to load: ")
                filename = input("Enter the JSON output filename: ")
                
                if os.path.isfile(json_filename):
                    try:
                        with open(json_filename, 'r', encoding='utf-8') as file:
                            json.load(file)
                        instance = CurrentDirectory.from_json(json_filename, filename)
                        return instance
                    except (FileNotFoundError, json.JSONDecodeError) as e:
                        print(f"Error loading JSON file: {str(e)}")

            instance = CurrentDirectory.from_json(json_filename)
            return instance
        else:
            print("Invalid choice. Please enter 'create' or 'load'.")

def get_action(instance: object):

    while 1:
        print('You can: ')
        print('  1. print all file extensions in directory tree')
        print('  2. create a json file representing directory tree')
        print('  3. exit')
        
        choice = input('Which one do you want to choose? ')
        if choice == '1':
            instance.print_file_extensions()
        elif choice == '2':
            instance.to_json()
        elif choice == '3':
            return