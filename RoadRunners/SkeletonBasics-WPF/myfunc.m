function [x] = myfunc(a,b,c,d) 
% a = Tid 
% b = puls
% c = vinklar_FHK
% d = vinklar_SHK
h = figure(1); set(gcf,'visible','off')

h.PaperUnits = 'inches';
h.PaperPosition = [0 0 14 4];
length(a)
length(b)
length(c)
length(d)

x = plot (a,b,a,c,a,d);
title('SuperGrafen');
xlabel('Tid');
ylabel('Vinkel');
 legend('Puls', 'Vinklar FHK','Vinklar SHK' , 'location', 'southwest');

saveas(h, 'Vinkelgraf.jpeg')
close(h);
end


