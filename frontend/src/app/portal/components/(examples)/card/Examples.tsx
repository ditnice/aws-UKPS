'use client'

import { Card } from '@nice-digital/nds-card'
import { Grid, GridItem } from '@nice-digital/nds-grid'

import { Example } from '../../_components/Example'

export function Examples() {
  return (
    <>
      <Example title="Card list">
        <ul className="list--unstyled">
          <li>
            <Card
              headingText="Card title 1"
              link={{ destination: '/test' }}
              summary="Lorem ipsum dolor sit amet, consectetur adipiscing elit. Fusce sed nisi enim. In nec lorem ac est cursus sollicitudin molestie vel nunc."
            />
          </li>
          <li>
            <Card
              headingText="Card title 2"
              summary="In rhoncus, urna sollicitudin blandit interdum, risus mauris malesuada magna, vitae maximus mauris leo ut elit. Integer maximus, nisi at congue volutpat, arcu diam finibus eros, quis tincidunt massa lacus nec ante."
            />
          </li>
        </ul>
      </Example>
      <Example title="Card grid">
        <Grid equalHeight>
          <GridItem cols={12} sm={{ cols: 4 }}>
            <Card
              headingText="Card title 1"
              link={{
                destination: 'https://www.example.com',
              }}
              summary="Lorem ipsum dolor sit amet, consectetur adipiscing elit. Fusce sed nisi enim. In nec lorem ac est cursus sollicitudin molestie vel nunc."
            />
          </GridItem>
          <GridItem cols={12} sm={{ cols: 4 }}>
            <Card
              headingText="Card title 2"
              link={{
                destination: 'https://www.example.com',
              }}
              summary="Nulla risus erat, maximus id semper ut, vulputate non nisl. Pellentesque sed luctus enim."
            />
          </GridItem>
          <GridItem cols={12} sm={{ cols: 4 }}>
            <Card
              headingText="Card title 3"
              link={{
                destination: 'https://www.example.com',
              }}
              summary="Integer maximus, nisi at congue volutpat, arcu diam finibus eros, quis tincidunt massa lacus nec ante."
            />
          </GridItem>
        </Grid>
      </Example>
    </>
  )
}
