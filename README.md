# RandomImageAPI
A random image api based on C# asp.net core.

## Usage

### Usage 1
First, use ```--GenerateList``` and ```--ImageFolder=``` to generate a file list. Will saved as ```file_list.json```  

Like ```./RandomImageAPI --ImageFolder=/home/kusu/images --GenerateList```

Then,

``
./RandomImageAPI --urls=http://ip:port --RedirectURL=http://whateveryouwant
``

Default listening ip:port is localhost:5000

And you need to put your image files under whateveryouwant/images

**You Cannot Left the RedirectURL empty.**

So now you can copy your images to other servers (or use other domains);

or copy RandomImageAPI and ```file_list.json``` to other servers.

**You should know that these two should place in the same folder rather than seperating them.**


### Usage 2 (Not Recommended)

```
./RandomImageAPI --urls=http://ip:port --ImageFolder=WhereIsYourImages --SelfHosted
```

Use this project in this way is only recommended for those who only has one server and only one domain and cannot create a new subdomain.


