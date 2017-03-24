# Testing your Mobile Application

There is nothing that causes more problems than when a developer works on testing.  Testing a cross-platform client-server application across all the permutations that are possible is hard work.  You will spend more time on developing tests than on writing code.  Much of what is asked, however, is not required.  That is primarily because most people want to test the entire stack.  There are generally minimal custom code in the backend, so that can significantly reduce the amount of tests you write.

In this section, we will look at what it takes to do unit tests for your mobile backend and the mobile app, together with an end-to-end testing capability that allows you to test your application on many devices at once.

## Testing your Mobile Backend

Most of the code within the mobile backend is pulled from libraries - ASP.NET, Entity Framework and Azure Mobile Apps.  These libraries are already tested before release and there is not much you can do about bugs other than reporting them (although Azure Mobile Apps does accept fixes as well).  As a result, you should concentrate your testing on the following areas:

*  Filters, Transforms and Actions associated with your table controllers.
*  Custom APIs.

In addition, your mobile backend will come under a lot of strain after you go to production.  You should plan on a load test prior to each major release in a staging environment that is identical to your production environment.  Never do a load test on your production environment after you have users, as such testing will affect your users.

### Unit Testing

### Load Testing

## Testing your Mobile Client

### Introduction to Mobile Client Testing

### Using Mock Data Services

### Unit Testing



## End to End Testing
### Introduction to Xamarin Test Cloud
