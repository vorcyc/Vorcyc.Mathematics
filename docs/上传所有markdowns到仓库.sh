https://github.com/vorcyc/Vorcyc.Mathematics.wiki.git

#!/bin/bash

# 设置变量
USERNAME="vorcyc"
REPOSITORY="Vorcyc.Mathematics"
WIKI_REPO="https://github.com/$USERNAME/$REPOSITORY.wiki.git"
MARKDOWN_DIR=""C:/Users/cyclo/Desktop/markdowns""

# 克隆Wiki仓库
git clone $WIKI_REPO
cd $REPOSITORY.wiki

# 复制Markdown文件
cp $MARKDOWN_DIR/*.md .

# 提交并推送更改
git add .
git commit -m "Add multiple markdown files"
git push
