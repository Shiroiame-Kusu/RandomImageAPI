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

## Other Arguments

### ``` --Seperate=<type> ```

```<type>``` can be ``` auto ``` , ``` manual ```.

if ``` --Seperate=auto ``` , the API will automatically detect the device type (pc or mobile) and return them a image based on the image length-width ratio.

else if ``` --Seperate=manual ``` , you need to manually divide your images into two folders under the ```$ImageFolder``` ,if $ImageFolder is default (./images) , then there should be two folders like:  
``` ./images/pc/ ``` ``` ./images/mobile ```

### ``` --APISeperated ```

If this is enabled, you now cannot get images from ``` http://ip:port/api ``` but from ``` http://ip:port/api/pc ``` and ``` http://ip:port/api/mobile ```