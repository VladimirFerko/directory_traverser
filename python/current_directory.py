import os
import json

class CurrentDirectory:
    def __init__(self, path: str, filename: str):
        self.path = path
        self.subdir_instances = []
        self.filename = filename
        self.get_subdirs()
        self.result = self.obj_to_dict()
    
    def __str__(self):
        return self.path
    
    @property
    def files(self):
        return [item for item in self.get_wd_files() if os.path.isfile(os.path.join(self.path, item))]
    
    @property
    def directories(self):
        return [item for item in self.get_wd_files() if os.path.isdir(os.path.join(self.path, item))]
    
    
    def get_wd_files(self):
        return os.listdir(self.path)
    
    def get_file_extension(self, filename):
        return os.path.splitext(filename)[1]
    
    def get_subdirs(self):
        for subdir in self.directories:
            try:
                subdir_instance = CurrentDirectory(os.path.join(self.path, subdir), self.filename)
                subdir_instance.get_subdirs()
                self.subdir_instances.append(subdir_instance)
            except PermissionError as err:
                print(err)

    def get_all_file_extensions(self):
        extensions = []
        for root, _, files in os.walk(self.path):
            for file in files:
                file_extension = self.get_file_extension(os.path.join(root, file))
                if file_extension not in extensions:
                    extensions.append(file_extension)
        return extensions
    
    def print_file_extensions(self):
        print(f'Path of your directory is: {self.path}')
        print('In this directory were found files with suffixes: ')

        extensions = self.get_all_file_extensions()
        for extension in extensions:
            print(f'\t- {extension}')

    def obj_to_dict(self):
        result = {
            "path": self.path,
            "files": self.files,  # Include the list of files directly
            "subdirectories": [subdir.obj_to_dict() for subdir in self.subdir_instances]
        }
        return result

    def to_json(self):
        with open(self.filename, 'w', encoding="utf-8") as file:
            json.dump(self.result, file, indent=4, separators=(',', ': '))

    @classmethod
    def from_json(cls, json_filename, output):
        with open(json_filename, 'r', encoding='utf-8') as file:
            json_data = json.load(file)
        
        path = json_data['path']
        instance = cls(path, output)
        
        # Recursively create instances for subdirectories
        for subdir_data in json_data['subdirectories']:
            subdir_instance = cls.from_dict(subdir_data, json_filename)
            instance.subdir_instances.append(subdir_instance)
        
        return instance

    @classmethod
    def from_dict(cls, data, json_filename):
        path = data['path']
        instance = cls(path, json_filename)
        
        # Recursively create instances for subdirectories
        for subdir_data in data['subdirectories']:
            subdir_instance = cls.from_dict(subdir_data, json_filename)
            instance.subdir_instances.append(subdir_instance)
        
        return instance
