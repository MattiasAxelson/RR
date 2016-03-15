function [b] = myfunc(a) 

b = plot (a);

title('Data');
xlabel('Tid');
ylabel('Vinkel och puls');

datetick(x,'HH');
end
