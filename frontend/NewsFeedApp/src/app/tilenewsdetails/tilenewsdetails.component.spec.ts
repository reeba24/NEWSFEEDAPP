import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TilenewsdetailsComponent } from './tilenewsdetails.component';

describe('TilenewsdetailsComponent', () => {
  let component: TilenewsdetailsComponent;
  let fixture: ComponentFixture<TilenewsdetailsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TilenewsdetailsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TilenewsdetailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
