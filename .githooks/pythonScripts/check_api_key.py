#!/usr/local/bin/python3

'''
Name:        check_api_key.py

Purpose:
This script parses files staged for commit to ensure
that they do not contain an API key.

Notes:

    Unity: apiKey:  APIKeyValue

    Block Commit:
        * Anything that contains AAPK
        * apiKey: APIKeyString - API key is set to any none empty value
        * apiKey: (1"asdf"df*) - API key is invalid, though may still contain sensitive information

    Allow Commit:
        * apiKey: - empty value
        * apiKey: "YOUR_API_KEY" - Citra requested snippet
        * apiKey: not found in any staged files
'''
import re
import argparse

#-------------------------------------------------------------------------------
# Global Variables
#-------------------------------------------------------------------------------

content = []

unity_apiKey_argument_regex = r"apiKey[\s]*\:[\s]*([\sa-zA-Z0-9_\"\'\-]*)"
# REGEX explanation: apiKey: {0+ spaces}{0+ alphanumeric characters}{0+ underscores}{0+ quotes}

valid_variable_regex = r"[_a-zA-Z][_a-zA-Z0-9]*[a-zA-Z0-9]"
# Starts with an alphabetical character or underscore, contains zero or more alphabetical characters or underscores then ends with an alphanumeric character

#-------------------------------------------------------------------------------
# Functions
#-------------------------------------------------------------------------------

def read_file(args):
    global content

    # Check if file was passed
    if not args.input:
        print(0)
        return

    source = args.input

    # try to open input file
    try:
        with open(source, 'r', encoding='utf-8') as file:
            content = file.readlines()
    except:
        # This file was most likely deleted.
        # Regardless, IO errors are not API keys and this should pass.
        print(0)
        return

    # for each line, parse line
    for i in range(len(content)):
        if "AAPK" in content[i]:
            print(i+1) # BLOCK anything with AAPK to be overly cautious
            return
        
        if "apiKey:" in content[i]:
            ApiKey_argument = re.search(unity_apiKey_argument_regex, content[i]).group(1)
            argument_value = check_argument(ApiKey_argument, i)+1
            if argument_value > 0:
                print(argument_value)
                return
            continue

    print(0) # ALLOW, API key not found anywhere
    return

#-------------------------------------------------------------------------------

def check_argument(ApiKey_argument: str, i: int) -> int: # returns 0 if ALLOW, else line_num if BLOCK.
    if not ApiKey_argument:
        return -1 # ALLOW, apikey is not set
    
    if ApiKey_argument[1:-1] == "YOUR_API_KEY":
        return -1 # ALLOW, apikey is Citra requested snippet

    if not re.match(valid_variable_regex, ApiKey_argument):
        return i # BLOCK, API key not a valid variable, though may still be sensitive information. For instance "-AAPK{...}"

    else:
        return i # BLOCK, API key is set in editor
    
    # We return i+1 to indicate the line number where the API key is defined, because line numbers are not zero indexed

#-------------------------------------------------------------------------------
# main process
#-------------------------------------------------------------------------------
def parse_command_line():

    parser = argparse.ArgumentParser(description="Format include lines in source code.")
    parser.add_argument("input", default=None, help="File to parse")
    args = parser.parse_args()

    return args

#-------------------------------------------------------------------------------
def main_process():
    args = parse_command_line()
    read_file(args)

#-------------------------------------------------------------------------------
if __name__ == '__main__':
    main_process()