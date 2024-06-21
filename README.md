## Environment:  
- .NET version: 6.0

## Read-Only Files:   
- FileProcessing.Tests/IntegrationTests.cs

## Data:  
Example of a StatisticalModel data JSON object:
```
{
 "AverageRate" : 30 
 "MaxRate" : 40 
 "MinRate" : 20 
}
```

## Requirements:

A company is launching a new service that works with XML files, that contains information about employees, and procures analytics from that. A file service needs to be created, and as part of this challenge, you are required to come up with a service to maintain these XML files.

As step 1, create a service that supports get statistics based on XML files. The Test XML file contains employee data such as First Name, Last Name, Email, Rate, etc.. Another API endpoint should add to the file user collection and return processed files with added users.

The XML has the following structure: 
- first_name - First Name of the employee. [STRING]
- last_name - Last Name of the employee. [STRING]
- email - Email of the Employee. [STRING]
- rate - Rate per hour. [INTEGER]
- id - Unique ID of the employee. [INTEGER]

## Example XML Structure:
```
<UserCollection>
    <User>
        <id>1</id>
        <first_name>Zed</first_name>
        <last_name>Grassi</last_name>
        <email>zgrassi0@youku.com</email>
        <rate>20</rate>
    </User>
</UserCollection>
```

## The following APIs need to be implemented:

 The following APIs need to be implemented: 

Getting analytics from file - POST request should be created to add analyze file. The API endpoint would be api/analyze. The request body contains the XML file (byte array content) in body. HTTP response should be Status200OK.

Adding employee collection to file - POST request should be created to add employees to the file. The API endpoint would be api/adduser. The request body contains "content" as a string(file content in base64) and employee collection. HTTP response should be Status200OK.

 

Also, TestMiddlewareController contains 2 implemented GET methods. with next routes: /api/testmiddleware/token and /api/testmiddleware/notoken. Need to create middleware that helps to:

1. Return status code 403 forbidden when call /api/testmiddleware/token without token.

2. Return status code 200 when call /api/testmiddleware/notoken without token.

3. Return status code 200 when call /api/testmiddleware/token?token=12345678 without token.



PS. TestMiddlewareController is read-only and can't be changed.
