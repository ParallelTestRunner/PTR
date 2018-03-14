@Login
Feature: Login
	Testing of Login page

Scenario: 1001- User should not be able to login with incorrect user name and password
 Given I open the login page of the website
 When I enter wrongUsername as user name
 And I enter wrongPassword as my password
 And I click the login button
 Then I can see the message Invalid username or password!

Scenario: 1002- User should be able to login with correct user name and password
 Given I open the login page of the website
 When I enter Luke as user name
 And I enter Skywalker as my password
 And I click the login button
 Then I am on the record page