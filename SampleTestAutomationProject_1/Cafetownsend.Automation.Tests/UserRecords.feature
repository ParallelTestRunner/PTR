@UserRecords
Feature: UserRecords
	Testing of UserRecords page

Scenario: 2001- User should be able to add a new record
 Given I open the login page of the website
 When I enter Luke as user name
 And I enter Skywalker as my password
 And I click the login button
 Then I am on the record page
 And I can create a new record with a random name

Scenario: 2002- User should be able to update a record
 Given I open the login page of the website
 When I enter Luke as user name
 And I enter Skywalker as my password
 And I click the login button
 Then I am on the record page
 And I can create a new record with a random name
 And I can also update that record

Scenario: 2003- User should be able to delete a record
 Given I open the login page of the website
 When I enter Luke as user name
 And I enter Skywalker as my password
 And I click the login button
 Then I am on the record page
 And I can create a new record with a random name
 And I can also delete that record
