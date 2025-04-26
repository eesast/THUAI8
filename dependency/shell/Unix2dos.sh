#!/bin/bash
function lsdir()
{
for i in `ls`;
do
        if [ -d $i ]
        then
                pushd ./$i
                lsdir
                popd
        else
                unix2dos $i
        fi
done
}
yum -y install unix2dos
cd $1
lsdir