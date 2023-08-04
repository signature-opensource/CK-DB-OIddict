# Frequent Abnormalities and Queries

- Why do I get an html form as a response when sending the consent form to the backend ?

You are probably using `fetch` or such. You must provide an actual form, as done by the html component.

If you don't do so, you will fail the backchannel and weird behavior is going to happen.

- I want a custom behavior on the server and the package already provides behavior that I cannot override and / or have unwanted side effect.

You can use the package CK.DB.OIddict instead of CK.DB.AspNet.OIddict.
