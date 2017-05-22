for i=0:52
   img=imread(strcat('Screenshot',num2str(i),'.png')); 
   imwrite(img,strcat('Screenshot',num2str(i),'.jpg'));
end