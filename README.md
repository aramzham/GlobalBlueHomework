# GlobalBlueHomework
As an API user, I would like to calculate Net, Gross, VAT amounts for purchases in Austria so that I can use correctly calculated purchase data.

For the ease of use you can run the application and apply one of those:
- swagger at url __https://localhost:7029/swagger/index.html__
- postman at url __https://localhost:7029/vat-calculator__ with different objects in the body
<img src="https://user-images.githubusercontent.com/25085025/231895086-7b9a5964-64b6-4320-b315-3e39b7acb56d.png" style="width:30%;height:30%;"/>

Overall the app is doing something similar to <a href="https://www.calkoo.com/en/vat-calculator">this value added tax calculator</a>. Here in this project you'll only find the backend part of it.
<img src="https://user-images.githubusercontent.com/25085025/231895903-6fc6e591-9f9e-4312-b6f0-1ffb0c4fc978.png" style="width:30%;height:30%;"/>

Set in motion with:
1. .Net 7.0 minimal API
2. <a href="https://docs.fluentvalidation.net/en/latest/">Fluent Validation</a>
3. <a href="https://nsubstitute.github.io/">NSubstitute</a>
4. <a href="https://github.com/bchavez/Bogus">Bogus</a>
