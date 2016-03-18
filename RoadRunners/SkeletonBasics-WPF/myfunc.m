function [x] = myfunc(a,b,c) 
% a = Tid 
% B = vinklar
% C = minsta vinkel
figure(1)

x = plot (a,b,a,c);
title('SuperGrafen');
xlabel('Tid');
ylabel('Vinkel');

%subplot(2,1,2);
% plot(b,'r');



end
